using Microsoft.Diagnostics.Runtime;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ImageWatchCSharp
{
    public class MatCLRMDReader : IDisposable
    {
        private DataTarget _dataTarget;
        private ClrRuntime _runtime;
        private ClrHeap _heap;
        private int _currentPid = -1;
        private bool _disposed = false;

    
        public IDataReader Reader => _dataTarget?.DataReader;

        public bool EnsureRuntime(int processId)
        {
            if (processId <= 0) return false;

            if (_currentPid == processId && _heap != null)
                return true;

            CleanupRuntime();

            try
            {
                _dataTarget = DataTarget.AttachToProcess(processId, false);
                
                if (_dataTarget.ClrVersions.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("目标进程没有CLR运行时");
                    return false;
                }

                _runtime = _dataTarget.ClrVersions[0].CreateRuntime();
                _heap = _runtime.Heap;
                _currentPid = processId;

                System.Diagnostics.Debug.WriteLine($"CLRMD成功连接到进程 {processId}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CLRMD连接失败: {ex.Message}");
                CleanupRuntime();
                return false;
            }
        }
        public MatHeader ReadMatHeader(string cvPtrValue)
        {
            if (_heap == null || string.IsNullOrEmpty(cvPtrValue))
                return default;

            try
            {
                if (!TryParseObjectAddress(cvPtrValue, out ulong objAddress))
                {
                    return default;
                }
                MatHeader header=   ReadMatHeader(_runtime,objAddress);
                return header;
            }
            catch (Exception ex)
            {
                return default;
            }
        }
        unsafe MatHeader ReadMatHeader(ClrRuntime runtime, ulong nativePtr)
        {
            byte[] buffer = new byte[Marshal.SizeOf<MatHeader>()];

            int bytesRead = Reader.Read(nativePtr, buffer);
            if (bytesRead != buffer.Length)
                throw new InvalidOperationException("Failed to read full MatHeader.");

            fixed (byte* ptr = buffer)
            {
                return *(MatHeader*)ptr;
            }
        }
        public bool ReadMatData(MatHeader matHeader, IntPtr buffer, int stride)
        {
            if (_dataTarget == null || matHeader.data == IntPtr.Zero || buffer == IntPtr.Zero)
                return false;

            if (matHeader.rows <= 0 || matHeader.cols <= 0)
                return false;

            try
            {
                int actualStep = matHeader.ActualStep;
                
                for (int y = 0; y < matHeader.rows; y++)
                {
                    ulong srcAddress = (ulong)matHeader.data.ToInt64() + (ulong)(y * actualStep);
                    IntPtr dstAddress = new IntPtr((long)buffer + (long)y * stride);
                    
                    int bytesToRead = Math.Min(actualStep, stride);
                    byte[] rowData = new byte[bytesToRead];

                    int bytesRead = _dataTarget.DataReader.Read(srcAddress, rowData);
                    if (bytesRead < bytesToRead)
                    {
                        System.Diagnostics.Debug.WriteLine($"读取第{y}行失败，期望{bytesToRead}字节，实际{bytesRead}字节");
                        return false;
                    }

                    System.Runtime.InteropServices.Marshal.Copy(rowData, 0, dstAddress, bytesToRead);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取Mat数据失败: {ex.Message}");
                return false;
            }
        }

        public byte[] ReadMatDataToArray(MatHeader matHeader)
        {
            if (_dataTarget == null || matHeader.data == IntPtr.Zero)
                return null;

            if (matHeader.rows <= 0 || matHeader.cols <= 0)
                return null;

            try
            {
                return matHeader.ToContiguousBytes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"使用MatHeader.ToContiguousBytes失败，尝试手动读取: {ex.Message}");
                
                return ReadMatDataManually(matHeader);
            }
        }

        private byte[] ReadMatDataManually(MatHeader matHeader)
        {
            try
            {
                int actualStep = matHeader.ActualStep;
                int totalBytes = matHeader.rows * actualStep;
                byte[] data = new byte[totalBytes];

                for (int y = 0; y < matHeader.rows; y++)
                {
                    ulong srcAddress = (ulong)matHeader.data.ToInt64() + (ulong)(y * actualStep);
                    int offset = y * actualStep;
                    
                    byte[] rowData = new byte[actualStep];
                    int bytesRead = _dataTarget.DataReader.Read(srcAddress, rowData);
                    if (bytesRead < actualStep)
                    {
                        return null;
                    }

                    Array.Copy(rowData, 0, data, offset, actualStep);
                }

                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"手动读取Mat数据失败: {ex.Message}");
                return null;
            }
        }

        private MatHeader ReadMatFieldsToHeader(ulong objAddress, ClrType type)
        {
            var matHeader = new MatHeader();

            try
            {
                string[] possibleRowsFields = { "rows", "_rows", "Rows" };
                string[] possibleColsFields = { "cols", "_cols", "Cols" };
                string[] possibleTypeFields = { "type", "_type", "Type", "flags" };
                string[] possibleDataFields = { "data", "_data", "Data", "DataPointer" };
                string[] possibleStepFields = { "step", "_step", "Step" };

                foreach (var fieldName in possibleRowsFields)
                {
                    if (TryReadIntField(objAddress, type, fieldName, out int rows))
                    {
                        matHeader.rows = rows;
                        break;
                    }
                }

                foreach (var fieldName in possibleColsFields)
                {
                    if (TryReadIntField(objAddress, type, fieldName, out int cols))
                    {
                        matHeader.cols = cols;
                        break;
                    }
                }

                foreach (var fieldName in possibleTypeFields)
                {
                    if (TryReadIntField(objAddress, type, fieldName, out int flags))
                    {
                        matHeader.flags = flags;
                        break;
                    }
                }

                foreach (var fieldName in possibleDataFields)
                {
                    if (TryReadPointerField(objAddress, type, fieldName, out ulong dataPtr))
                    {
                        matHeader.data = new IntPtr((long)dataPtr);
                        break;
                    }
                }

                matHeader.dims = 2; 
                matHeader.datastart = matHeader.data;
                
                if (TryReadStepPointer(objAddress, type, out IntPtr stepPtr))
                {
                    matHeader.step_p = stepPtr;
                }

                int expectedBytes = matHeader.rows * matHeader.ExpectedStep;
                matHeader.dataend = new IntPtr(matHeader.data.ToInt64() + expectedBytes);
                matHeader.datalimit = matHeader.dataend;

                return matHeader;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        private bool TryReadIntField(ulong objAddress, ClrType type, string fieldName, out int value)
        {
            value = 0;
            try
            {
                var field = type.GetFieldByName(fieldName);
                if (field == null) return false;

                ulong fieldAddress = objAddress + (ulong)field.Offset;
                
                switch (field.ElementType)
                {
                    case ClrElementType.Int32:
                        int intValue = 0;
                        if (_dataTarget.DataReader.Read(fieldAddress, out intValue))
                        {
                            value = intValue;
                            return true;
                        }
                        break;
                    case ClrElementType.UInt32:
                        uint uintValue = 0;
                        if (_dataTarget.DataReader.Read(fieldAddress, out uintValue))
                        {
                            value = (int)uintValue;
                            return true;
                        }
                        break;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool TryReadPointerField(ulong objAddress, ClrType type, string fieldName, out ulong value)
        {
            value = 0;
            try
            {
                var field = type.GetFieldByName(fieldName);
                if (field == null) return false;

                ulong fieldAddress = objAddress + (ulong)field.Offset;
                
                if (_dataTarget.DataReader.ReadPointer(fieldAddress, out value))
                {
                    return value != 0;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool TryReadStepPointer(ulong objAddress, ClrType type, out IntPtr stepPtr)
        {
            stepPtr = IntPtr.Zero;
            
            string[] possibleStepFields = { "step", "_step", "Step" };
            
            foreach (var fieldName in possibleStepFields)
            {
                try
                {
                    var field = type.GetFieldByName(fieldName);
                    if (field == null) continue;

                    ulong fieldAddress = objAddress + (ulong)field.Offset;
                    if (field.ElementType == ClrElementType.Pointer)
                    {
                        ulong stepArrayPtr = 0;
                        if (_dataTarget.DataReader.ReadPointer(fieldAddress, out stepArrayPtr) && stepArrayPtr != 0)
                        {
                            stepPtr = new IntPtr((long)stepArrayPtr);
                            return true;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
            
            return false;
        }

        private bool TryParseObjectAddress(string addressStr, out ulong address)
        {
            address = 0;
            if (string.IsNullOrEmpty(addressStr)) return false;

            var hexMatch = Regex.Match(addressStr, @"0x[0-9a-fA-F]+");
            if (hexMatch.Success)
            {
                return ulong.TryParse(hexMatch.Value.Substring(2), 
                    System.Globalization.NumberStyles.HexNumber, null, out address);
            }
            var decMatch = Regex.Match(addressStr, @"\d+");
            if (decMatch.Success)
            {
                return ulong.TryParse(decMatch.Value, out address);
            }

            return false;
        }

        private bool IsMatType(ClrType type)
        {
            if (type?.Name == null) return false;
            
            string typeName = type.Name.ToLowerInvariant();
            return (typeName.Contains("mat") && 
                   (typeName.Contains("opencv") || typeName.Contains("opencvsharp"))) ||
                   typeName == "opencvsharp.mat" ||
                   typeName.EndsWith(".mat");
        }

        private int GetBytesPerPixel(int matType)
        {
            int depth = matType & 7;
            int channels = ((matType >> 3) & 511) + 1;

            int bytesPerChannel;
            switch (depth)
            {
                case 0: bytesPerChannel = 1; break; 
                case 1: bytesPerChannel = 1; break; 
                case 2: bytesPerChannel = 2; break; 
                case 3: bytesPerChannel = 2; break; 
                case 4: bytesPerChannel = 4; break; 
                case 5: bytesPerChannel = 4; break; 
                case 6: bytesPerChannel = 8; break; 
                case 7: bytesPerChannel = 2; break; 
                default: bytesPerChannel = 1; break;
            }

            return channels * bytesPerChannel;
        }

        private void CleanupRuntime()
        {
            _heap = null;
            _runtime?.Dispose();
            _runtime = null;
            _dataTarget?.Dispose();
            _dataTarget = null;
            _currentPid = -1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CleanupRuntime();
                }
                _disposed = true;
            }
        }

        ~MatCLRMDReader()
        {
            Dispose(false);
        }
    }
}