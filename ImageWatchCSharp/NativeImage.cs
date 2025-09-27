using ImageWatchCSharp.Bind;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;

namespace ImageWatchCSharp
{
    internal class NativeImage : BindableBase, IDisposable
    {

        private const int CV_MAT_CONT_FLAG = 0x4000;     
        private const int CV_MAT_TYPE_MASK = 0x00000FFF; 

        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public MatType Type
        {
            get
            {
                return flags & CV_MAT_TYPE_MASK;
            }
        }
        public IntPtr Data { get; private set; }
        public long Step { get; private set; }
        public int Channels { get; private set; }
        public int Depth { get; private set; }

        public bool IsContinuous
        {
            get
            {
                return (flags & CV_MAT_CONT_FLAG) != 0;
            }
        }

        private int flags = 0;      
        private IntPtr _hProcess;
        private bool _isDisposed;
        private IntPtr _mappedFile;
        private IntPtr _mappedView;
        private InteropBitmap _interopBitmap;
        private long _actualStep;

        private IntPtr _cvPtr = IntPtr.Zero;  
        private IntPtr _lastDataPtr = IntPtr.Zero;
        private int _lastRows = -1;
        private int _lastCols = -1;
        private int _lastFlags = -1;
        private long _lastStep = -1;
        private uint _lastDataChecksum = 0;
        private DateTime _lastRefreshTime = DateTime.MinValue;

        public NativeImage(IntPtr hProcess, IntPtr CvPtr)
        {
            if (CvPtr == IntPtr.Zero)
                return;

            _hProcess = hProcess;
            _cvPtr = CvPtr;

            RefreshMatData();
        }

        public bool HasMatChanged()
        {
            if (_cvPtr == IntPtr.Zero || _hProcess == IntPtr.Zero)
                return false;

            try
            {
                var currentFlags = ReadInt32(_hProcess, _cvPtr);
                var currentRows = ReadInt32(_hProcess, _cvPtr + 8);
                var currentCols = ReadInt32(_hProcess, _cvPtr + 12);
                var currentDataPtr = ReadIntPtr(_hProcess, _cvPtr + 16);

                IntPtr step_p = ReadIntPtr(_hProcess, _cvPtr + 72);
                var currentStep = step_p != IntPtr.Zero ? ReadInt32(_hProcess, step_p) : 0;

                bool hasStructuralChange = _lastDataPtr != currentDataPtr ||
                                         _lastRows != currentRows ||
                                         _lastCols != currentCols ||
                                         _lastFlags != currentFlags ||
                                         _lastStep != currentStep;

                return hasStructuralChange;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool HasDataContentChanged()
        {
            if (Data == IntPtr.Zero || Rows <= 0 || Cols <= 0)
                return false;

            try
            {
                var currentChecksum = CalculateDataChecksum();
                bool hasChanged = _lastDataChecksum != currentChecksum;

                if (hasChanged)
                {
                    _lastDataChecksum = currentChecksum;
                }

                return hasChanged;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private uint CalculateDataChecksum()
        {
            if (Data == IntPtr.Zero || Rows <= 0 || Cols <= 0)
                return 0;

            int totalBytes = Rows * Cols * Channels;
            int sampleSize = Math.Min(1024, totalBytes / 10);
            if (sampleSize < 16) sampleSize = Math.Min(16, totalBytes);

            byte[] sampleData = new byte[sampleSize];
            var gcHandle = GCHandle.Alloc(sampleData, GCHandleType.Pinned);

            try
            {
                IntPtr bytesRead;
                if (WinAPI.ReadProcessMemory(_hProcess, Data, gcHandle.AddrOfPinnedObject(), (IntPtr)sampleSize, out bytesRead))
                {
                    uint checksum = 0;
                    for (int i = 0; i < sampleSize; i++)
                    {
                        checksum = ((checksum << 1) | (checksum >> 31)) ^ sampleData[i];
                    }
                    return checksum;
                }
            }
            finally
            {
                gcHandle.Free();
            }

            return 0;
        }

        public bool SmartRefresh(bool checkContent = false)
        {
            if (_cvPtr == IntPtr.Zero)
                return false;

            bool needRefresh = HasMatChanged();

            if (!needRefresh && checkContent)
            {
                needRefresh = HasDataContentChanged();
            }

            if (needRefresh)
            {
                try
                {
                    RefreshMatData();

                    System.Diagnostics.Debug.WriteLine($"NativeImage已刷新: {Cols}x{Rows} at {DateTime.Now}");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SmartRefresh失败: {ex.Message}");
                }
            }

            return false;
        }


        private void RefreshMatData()
        {
            if (_cvPtr == IntPtr.Zero)
                return;

            try
            {
                DisposeImageResources();

                flags = ReadInt32(_hProcess, _cvPtr);
                int dims = ReadInt32(_hProcess, _cvPtr + 4);
                Rows = ReadInt32(_hProcess, _cvPtr + 8);
                Cols = ReadInt32(_hProcess, _cvPtr + 12);
                Data = ReadIntPtr(_hProcess, _cvPtr + 16);

                IntPtr datastart = ReadIntPtr(_hProcess, _cvPtr + 24);
                IntPtr dataend = ReadIntPtr(_hProcess, _cvPtr + 32);
                IntPtr datalimit = ReadIntPtr(_hProcess, _cvPtr + 40);
                IntPtr refcount = ReadIntPtr(_hProcess, _cvPtr + 48);

                IntPtr step_p = ReadIntPtr(_hProcess, _cvPtr + 72);
                Step = ReadInt32(_hProcess, step_p);

                Depth = Type.Depth;      
                Channels = Type.Channels;

                _lastDataPtr = Data;
                _lastRows = Rows;
                _lastCols = Cols;
                _lastFlags = flags;
                _lastStep = Step;
                _lastRefreshTime = DateTime.Now;

                if (Rows > 0 && Cols > 0 && Data != IntPtr.Zero)
                {
                    CreateSharedMemory();

                    _lastDataChecksum = CalculateDataChecksum();
                }

                NotifyPropertyChanged(nameof(Rows));
                NotifyPropertyChanged(nameof(Cols));
                NotifyPropertyChanged(nameof(Type));
                NotifyPropertyChanged(nameof(Channels));
                NotifyPropertyChanged(nameof(Depth));
                NotifyPropertyChanged(nameof(Data));
                NotifyPropertyChanged(nameof(Step));
                NotifyPropertyChanged(nameof(Image));
            }
            catch (Exception ex)
            {
            }
        }

        private void DisposeImageResources()
        {
            _interopBitmap = null;
            _image = null;

            if (_mappedView != IntPtr.Zero)
            {
                WinAPI.UnmapViewOfFile(_mappedView);
                _mappedView = IntPtr.Zero;
            }

            if (_mappedFile != IntPtr.Zero)
            {
                WinAPI.CloseHandle(_mappedFile);
                _mappedFile = IntPtr.Zero;
            }
        }

        public void ForceRefresh()
        {
            _lastDataPtr = IntPtr.Zero;
            _lastRows = -1;
            _lastCols = -1;
            _lastFlags = -1;
            _lastStep = -1;
            _lastDataChecksum = 0;

            SmartRefresh(true);
        }

        private int GetBytesPerPixel()
        {
            int bytesPerElement;
            switch (Depth)
            {
                case 0: bytesPerElement = 1; break;
                case 1: bytesPerElement = 1; break;
                case 2: bytesPerElement = 2; break;
                case 3: bytesPerElement = 2; break;
                case 4: bytesPerElement = 4; break;
                case 5: bytesPerElement = 4; break;
                case 6: bytesPerElement = 8; break;
                default: bytesPerElement = 1; break;
            }
            return bytesPerElement * Channels;
        }

        private PixelFormat GetPixelFormat()
        {
            if (Channels == 1)
                return PixelFormats.Gray8;
            else if (Channels == 2)
                return PixelFormats.Bgr24;
            else if (Channels == 3)
                return PixelFormats.Bgr24;
            else if (Channels == 4)
                return PixelFormats.Bgra32;
            else
                return PixelFormats.Default;
        }

        private InteropBitmap _image;
        public InteropBitmap Image
        {
            get
            {
                if (_image == null && _mappedView != IntPtr.Zero)
                {
                    try
                    {
                        _interopBitmap = Imaging.CreateBitmapSourceFromMemorySection(
                            _mappedFile,
                            Cols,
                            Rows,
                            GetPixelFormat(),
                            (int)_actualStep, 
                            0) as InteropBitmap;

                        _image = _interopBitmap;
                        NotifyPropertyChanged();
                    }
                    catch (Exception ex)
                    {
                    }
                }
                return _image;
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                DisposeImageResources();
                _isDisposed = true;
            }
        }

        private void CreateSharedMemory()
        {
            if (Rows <= 0 || Cols <= 0 || Data == IntPtr.Zero)
                return;

            int bytesPerPixel = GetBytesPerPixel();
            int rowBytes = Cols * bytesPerPixel; 
            bool needsPseudoColor = NeedsPseudoColor();

            if (needsPseudoColor)
            {
                int sourceDataSize = Rows * (int)Step;
                int destRowBytes = Cols * 3; 
                int destStep = (destRowBytes + 3) & ~3;
                int destDataSize = Rows * destStep;
                
                _actualStep = destStep; 
                
                IntPtr tempMappedFile = IntPtr.Zero;
                IntPtr tempMappedView = IntPtr.Zero;
                
                try
                {
                    string tempMapName = "TempImageWatchSharedMem_" + Guid.NewGuid().ToString("N");
                    tempMappedFile = WinAPI.CreateFileMapping(new IntPtr(-1), IntPtr.Zero,
                        PageProtection.ReadWrite, 0, (uint)sourceDataSize, tempMapName);
                    
                    if (tempMappedFile == IntPtr.Zero)
                        throw new Exception("Failed to create temporary file mapping");
                        
                    tempMappedView = WinAPI.MapViewOfFile(tempMappedFile,
                        FileMapAccess.Write, 0, 0, (UIntPtr)sourceDataSize);
                        
                    if (tempMappedView == IntPtr.Zero)
                        throw new Exception("Failed to map temporary view of file");
                    
                    IntPtr bytesRead;
                    if (IsContinuous)
                    {
                        WinAPI.ReadProcessMemory(_hProcess, Data, tempMappedView, (IntPtr)sourceDataSize, out bytesRead);
                    }
                    else
                    {
                        CopyNonContinuousData(tempMappedView, rowBytes);
                    }
                    
                    string destMapName = "ImageWatchSharedMem_" + Guid.NewGuid().ToString("N");
                    _mappedFile = WinAPI.CreateFileMapping(new IntPtr(-1), IntPtr.Zero,
                        PageProtection.ReadWrite, 0, (uint)destDataSize, destMapName);
                        
                    if (_mappedFile == IntPtr.Zero)
                        throw new Exception("Failed to create destination file mapping");
                        
                    _mappedView = WinAPI.MapViewOfFile(_mappedFile,
                        FileMapAccess.Write, 0, 0, (UIntPtr)destDataSize);
                        
                    if (_mappedView == IntPtr.Zero)
                        throw new Exception("Failed to map destination view of file");
                    
                    ApplyPseudoColor(tempMappedView, _mappedView, Cols, Rows, 
                        IsContinuous ? (int)Step : rowBytes, destStep);
                }
                finally
                {
                    if (tempMappedView != IntPtr.Zero)
                        WinAPI.UnmapViewOfFile(tempMappedView);
                        
                    if (tempMappedFile != IntPtr.Zero)
                        WinAPI.CloseHandle(tempMappedFile);
                }
            }
            else
            {
                if (IsContinuous)
                {
                    int dataSize = Rows * (int)Step;
                    _actualStep = Step;

                    CreateSharedMemoryAndCopy(dataSize, (mappedView) =>
                    {
                        IntPtr bytesRead;
                        WinAPI.ReadProcessMemory(_hProcess, Data, mappedView, (IntPtr)dataSize, out bytesRead);
                    });
                }
                else
                {
                    _actualStep = (rowBytes + 3) & ~3; // 4字节对齐
                    int dataSize = Rows * (int)_actualStep;

                    CreateSharedMemoryAndCopy(dataSize, (mappedView) =>
                    {
                        CopyNonContinuousData(mappedView, rowBytes);
                    });
                }
            }
        }

        private void CreateSharedMemoryAndCopy(int dataSize, Action<IntPtr> copyAction)
        {
            string mapName = "ImageWatchSharedMem_" + Guid.NewGuid().ToString("N");
            _mappedFile = WinAPI.CreateFileMapping(new IntPtr(-1), IntPtr.Zero,
                PageProtection.ReadWrite, 0, (uint)dataSize, mapName);

            if (_mappedFile == IntPtr.Zero)
                throw new Exception("Failed to create file mapping");

            _mappedView = WinAPI.MapViewOfFile(_mappedFile,
                FileMapAccess.Write, 0, 0, (UIntPtr)dataSize);

            if (_mappedView == IntPtr.Zero)
            {
                WinAPI.CloseHandle(_mappedFile);
                _mappedFile = IntPtr.Zero;
                throw new Exception("Failed to map view of file");
            }

            copyAction(_mappedView);
        }

        private void CopyNonContinuousData(IntPtr mappedView, int rowBytes)
        {
            IntPtr srcPtr = Data;
            IntPtr dstPtr = mappedView;

            for (int row = 0; row < Rows; row++)
            {
                IntPtr bytesRead;
                WinAPI.ReadProcessMemory(_hProcess, srcPtr, dstPtr, (IntPtr)rowBytes, out bytesRead);

                srcPtr = IntPtr.Add(srcPtr, (int)Step);
                dstPtr = IntPtr.Add(dstPtr, (int)_actualStep); 
            }
        }

        private bool Convert2ChannelToFalseColor(IntPtr mappedView, int width, int height, int originalStep)
        {
            if (Channels != 2)
                return false;

            try
            {
                int newBytesPerPixel = 3;
                int newRowBytes = width * newBytesPerPixel;
                int newStep = (newRowBytes + 3) & ~3;
                int newDataSize = height * newStep;

                byte[] tempBuffer = new byte[newDataSize];

                // 对每个像素应用伪彩色映射
                int bytesPerPixel = GetBytesPerPixel(); 
                int elementSize = bytesPerPixel / Channels; 

                unsafe
                {
                    byte* srcData = (byte*)mappedView.ToPointer();
                    fixed (byte* dstData = tempBuffer)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            byte* srcRow = srcData + y * originalStep;
                            byte* dstRow = dstData + y * newStep;

                            for (int x = 0; x < width; x++)
                            {
                                float ch1 = 0, ch2 = 0;

                                if (Depth == 5) 
                                {
                                    ch1 = *(float*)(srcRow + x * bytesPerPixel);
                                    ch2 = *(float*)(srcRow + x * bytesPerPixel + 4);
                                }
                                else if (Depth == 0)
                                {
                                    ch1 = srcRow[x * bytesPerPixel] / 255.0f;
                                    ch2 = srcRow[x * bytesPerPixel + 1] / 255.0f;
                                }
                                else if (Depth == 2) 
                                {
                                    ch1 = *(ushort*)(srcRow + x * bytesPerPixel) / 65535.0f;
                                    ch2 = *(ushort*)(srcRow + x * bytesPerPixel + 2) / 65535.0f;
                                }
                                else
                                {
                                    byte* ptr = srcRow + x * bytesPerPixel;
                                    ch1 = ptr[0] / 255.0f;
                                    ch2 = ptr[elementSize] / 255.0f;
                                }

                                HsvToBgr(ch1 * 360, ch2, 1.0f, out byte b, out byte g, out byte r);

                                dstRow[x * 3] = b;
                                dstRow[x * 3 + 1] = g;
                                dstRow[x * 3 + 2] = r;
                            }
                        }
                    }
                }

                Marshal.Copy(tempBuffer, 0, mappedView, tempBuffer.Length);

                _actualStep = newStep;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void HsvToBgr(float h, float s, float v, out byte b, out byte g, out byte r)
        {
            h = h % 360;
            if (h < 0) h += 360;

            s = Math.Max(0, Math.Min(1, s));
            v = Math.Max(0, Math.Min(1, v));

            int hi = (int)(h / 60) % 6;
            float f = h / 60 - hi;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            float rf, gf, bf;
            switch (hi)
            {
                case 0: rf = v; gf = t; bf = p; break;
                case 1: rf = q; gf = v; bf = p; break;
                case 2: rf = p; gf = v; bf = t; break;
                case 3: rf = p; gf = q; bf = v; break;
                case 4: rf = t; gf = p; bf = v; break;
                default: rf = v; gf = p; bf = q; break;
            }

            r = (byte)(rf * 255);
            g = (byte)(gf * 255);
            b = (byte)(bf * 255);
        }

        #region read memory
        private int ReadInt16(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[2];
            IntPtr bytesRead;
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                WinAPI.ReadProcessMemory(hProcess, address, handle.AddrOfPinnedObject(), (IntPtr)2, out bytesRead);
                return BitConverter.ToInt16(buffer, 0);
            }
            finally
            {
                handle.Free();
            }
        }
        private int ReadInt32(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4];
            IntPtr bytesRead;
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                WinAPI.ReadProcessMemory(hProcess, address, handle.AddrOfPinnedObject(), (IntPtr)4, out bytesRead);
                return BitConverter.ToInt32(buffer, 0);
            }
            finally
            {
                handle.Free();
            }
        }

        private long ReadInt64(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[8];
            IntPtr bytesRead;
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                WinAPI.ReadProcessMemory(hProcess, address, handle.AddrOfPinnedObject(), (IntPtr)8, out bytesRead);
                return BitConverter.ToInt64(buffer, 0);
            }
            finally
            {
                handle.Free();
            }
        }

        private IntPtr ReadIntPtr(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[IntPtr.Size];
            IntPtr bytesRead;
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                WinAPI.ReadProcessMemory(hProcess, address, handle.AddrOfPinnedObject(), (IntPtr)IntPtr.Size, out bytesRead);
                if (IntPtr.Size == 8)
                    return (IntPtr)BitConverter.ToInt64(buffer, 0);
                else
                    return (IntPtr)BitConverter.ToInt32(buffer, 0);
            }
            finally
            {
                handle.Free();
            }
        }
        #endregion

        public string GetOriginalPixelValueAtPoint(int x, int y)
        {
            if (Data == IntPtr.Zero || x < 0 || y < 0 || x >= Cols || y >= Rows)
                return string.Empty;
            try
            {
                int bytesPerPixel = GetBytesPerPixel();
                IntPtr pixelAddress = IntPtr.Add(Data, y * (int)Step + x * bytesPerPixel);
                
                switch (Depth)
                {
                    case 0: // CV_8U
                        {
                            byte[] values = ReadBytes(_hProcess, pixelAddress, Channels);
                            return FormatPixelValues(values);
                        }
                    case 1: // CV_8S
                        {
                            byte[] bytes = ReadBytes(_hProcess, pixelAddress, Channels);
                            sbyte[] values = Array.ConvertAll(bytes, b => unchecked((sbyte)b));
                            return FormatPixelValues(values);
                        }
                    case 2: // CV_16U
                        {
                            ushort[] values = new ushort[Channels];
                            for (int i = 0; i < Channels; i++)
                            {
                                values[i] = unchecked((ushort)ReadInt16(_hProcess, IntPtr.Add(pixelAddress, i * 2)));
                            }
                            return FormatPixelValues(values);
                        }
                    case 3: // CV_16S
                        {
                            short[] values = new short[Channels];
                            for (int i = 0; i < Channels; i++)
                            {
                                values[i] = unchecked((short)ReadInt16(_hProcess, IntPtr.Add(pixelAddress, i * 2)));
                            }
                            return FormatPixelValues(values);
                        }
                    case 4: // CV_32S
                        {
                            int[] values = new int[Channels];
                            for (int i = 0; i < Channels; i++)
                            {
                                values[i] = ReadInt32(_hProcess, IntPtr.Add(pixelAddress, i * 4));
                            }
                            return FormatPixelValues(values);
                        }
                    case 5: // CV_32F
                        {
                            float[] values = new float[Channels];
                            for (int i = 0; i < Channels; i++)
                            {
                                byte[] buffer = new byte[4];
                                IntPtr bytesRead;
                                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                                try
                                {
                                    WinAPI.ReadProcessMemory(_hProcess, IntPtr.Add(pixelAddress, i * 4), 
                                        handle.AddrOfPinnedObject(), (IntPtr)4, out bytesRead);
                                    values[i] = BitConverter.ToSingle(buffer, 0);
                                }
                                finally
                                {
                                    handle.Free();
                                }
                            }
                            return FormatPixelValues(values);
                        }
                    case 6: // CV_64F
                        {
                            double[] values = new double[Channels];
                            for (int i = 0; i < Channels; i++)
                            {
                                byte[] buffer = new byte[8];
                                IntPtr bytesRead;
                                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                                try
                                {
                                    WinAPI.ReadProcessMemory(_hProcess, IntPtr.Add(pixelAddress, i * 8), 
                                        handle.AddrOfPinnedObject(), (IntPtr)8, out bytesRead);
                                    values[i] = BitConverter.ToDouble(buffer, 0);
                                }
                                finally
                                {
                                    handle.Free();
                                }
                            }
                            return FormatPixelValues(values);
                        }
                    default:
                        return "No Support MatType";
                }
            }
            catch (Exception ex)
            {
                return "Read Pixel Error ";
            }
        }

        private string FormatPixelValues<T>(T[] values)
        {
            if (values == null || values.Length == 0)
                return string.Empty;
            
            if (values.Length == 1)
                return values[0].ToString();
            
            if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
            {
                return string.Join(", ", values.Select(v => v.ToString()));
            }
            else
            {
                return string.Join(", ", values.Reverse());
            }
        }

        private byte[] ReadBytes(IntPtr hProcess, IntPtr address, int count)
        {
            byte[] buffer = new byte[count];
            IntPtr bytesRead;
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                WinAPI.ReadProcessMemory(hProcess, address, handle.AddrOfPinnedObject(), (IntPtr)count, out bytesRead);
                return buffer;
            }
            finally
            {
                handle.Free();
            }
        }

        private bool NeedsPseudoColor()
        {
            if (Depth == 0 && (Channels == 1 || Channels == 3))
                return false;
                
            return true;
        }

        private void ApplyPseudoColor(IntPtr sourceMappedView, IntPtr destMappedView, 
            int width, int height, int sourceStep, int destStep)
        {
            try
            {
                using (var mapper = PseudoColorMapperInterop.CreateMapper(PseudoColorMode.JET))
                {
                    if (mapper == null)
                        throw new Exception("Failed to create pseudo color mapper");
                    
                    if (Depth == 5 || Depth == 6) // CV_32F or CV_64F
                    {
                        mapper.SetColorMode(PseudoColorMode.HOT); 
                    }
                    
                    bool result = mapper.ApplyPseudoColor(
                        sourceMappedView,
                        width,
                        height,
                        Channels,
                        destMappedView,
                        -1 
                    );
                    
                    if (!result)
                        throw new Exception("Failed to apply pseudo color");
                }
            }
            catch (Exception ex)
            {
                FallbackToPseudoColor(sourceMappedView, destMappedView, width, height, sourceStep, destStep);
            }
        }

        private void FallbackToPseudoColor(IntPtr sourceMappedView, IntPtr destMappedView,
            int width, int height, int sourceStep, int destStep)
        {
            byte[] rowBuffer = new byte[sourceStep];
            byte[] outBuffer = new byte[destStep];
            
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(IntPtr.Add(sourceMappedView, y * sourceStep), rowBuffer, 0, sourceStep);
                
                for (int x = 0; x < width; x++)
                {
                    int srcPos = x * Channels * (GetBytesPerPixel() / Channels);
                    int dstPos = x * 3; 
                    
                    double pixelValue = 0;
                    
                    if (Depth <= 1) 
                    {
                        for (int c = 0; c < Channels; c++)
                        {
                            pixelValue += rowBuffer[srcPos + c];
                        }
                        pixelValue /= Channels;
                    }
                    else
                    {
                        pixelValue = NormalizeValueToDouble(rowBuffer, srcPos, Depth);
                    }
                    
                    byte r, g, b;
                    
                    double normalized = Math.Max(0, Math.Min(1.0, pixelValue / 255.0));
                    
                    if (normalized < 0.25)
                    {
                        b = 255;
                        g = (byte)(normalized * 4 * 255);
                        r = 0;
                    }
                    else if (normalized < 0.5)
                    {
                        b = (byte)((0.5 - normalized) * 4 * 255);
                        g = 255;
                        r = 0;
                    }
                    else if (normalized < 0.75)
                    {
                        b = 0;
                        g = 255;
                        r = (byte)((normalized - 0.5) * 4 * 255);
                    }
                    else
                    {
                        b = 0;
                        g = (byte)((1.0 - normalized) * 4 * 255);
                        r = 255;
                    }
                    
                    outBuffer[dstPos] = b;
                    outBuffer[dstPos + 1] = g;
                    outBuffer[dstPos + 2] = r;
                }
                
                Marshal.Copy(outBuffer, 0, IntPtr.Add(destMappedView, y * destStep), destStep);
            }
        }

        private double NormalizeValueToDouble(byte[] buffer, int offset, int depth)
        {
            switch (depth)
            {
                case 0: // CV_8U
                    return buffer[offset];
                case 1: // CV_8S
                    return unchecked((sbyte)buffer[offset]) + 128;
                case 2: // CV_16U
                    return BitConverter.ToUInt16(buffer, offset) / 256.0;
                case 3: // CV_16S
                    return (BitConverter.ToInt16(buffer, offset) + 32768) / 256.0;
                case 4: // CV_32S
                    {
                        int val = BitConverter.ToInt32(buffer, offset);
                        return ((val - int.MinValue) * 255.0) / uint.MaxValue;
                    }
                case 5: // CV_32F
                    {
                        float val = BitConverter.ToSingle(buffer, offset);
                        return MapFloatToNormalizedValue(val);
                    }
                case 6: // CV_64F
                    {
                        double val = BitConverter.ToDouble(buffer, offset);
                        return MapFloatToNormalizedValue((float)val);
                    }
                default:
                    return 0;
            }
        }

        private double MapFloatToNormalizedValue(float value)
        {
            float min = 0, max = 1;
            
            if (value >= 0 && value <= 1)
            {
                min = 0;
                max = 1;
            }
            else if (value >= -1 && value <= 1)
            {
                min = -1;
                max = 1;
                value = (value + 1) / 2; 
                return value * 255;
            }
            else
            {
       
                min = 0;
                max = 255;
            }
            
            float normalized = (Math.Max(Math.Min(value, max), min) - min) / (max - min);
            return normalized * 255;
        }

        public InteropBitmap CreateColorBar(int width = 256, int height = 32)
        {
            if (width <= 0 || height <= 0)
                return null;
                
            try
            {
                string mapName = "ColorBarSharedMem_" + Guid.NewGuid().ToString("N");
                int dataSize = width * height * 3; 
                
                IntPtr mappedFile = WinAPI.CreateFileMapping(new IntPtr(-1), IntPtr.Zero,
                    PageProtection.ReadWrite, 0, (uint)dataSize, mapName);
                    
                if (mappedFile == IntPtr.Zero)
                    return null;
                    
                IntPtr mappedView = WinAPI.MapViewOfFile(mappedFile,
                    FileMapAccess.Write, 0, 0, (UIntPtr)dataSize);
                    
                if (mappedView == IntPtr.Zero)
                {
                    WinAPI.CloseHandle(mappedFile);
                    return null;
                }
                
                using (var mapper = PseudoColorMapperInterop.CreateMapper(Depth == 5 || Depth == 6 ? 
                                                                        PseudoColorMode.HOT : PseudoColorMode.JET))
                {
                    if (mapper == null)
                    {
                        WinAPI.UnmapViewOfFile(mappedView);
                        WinAPI.CloseHandle(mappedFile);
                        return null;
                    }
                    
                    bool result = mapper.GetColorBar(mappedView, width, height);
                    if (!result)
                    {
                        WinAPI.UnmapViewOfFile(mappedView);
                        WinAPI.CloseHandle(mappedFile);
                        return null;
                    }
                }
                
                var colorBar = Imaging.CreateBitmapSourceFromMemorySection(
                    mappedFile,
                    width,
                    height,
                    PixelFormats.Bgr24,
                    width * 3, // step = width * 3 bytes
                    0) as InteropBitmap;
                    
                return colorBar;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public PseudoColorMode ColorMode
        {
            get
            {
                if (Depth == 5 || Depth == 6) // CV_32F or CV_64F
                    return PseudoColorMode.HOT;
                else
                    return PseudoColorMode.JET;
            }
        }
    }
}
