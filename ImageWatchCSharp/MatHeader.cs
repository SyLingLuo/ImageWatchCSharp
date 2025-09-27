using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageWatchCSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MatHeader
    {
        public int flags;       
        public int dims;        
        public int rows;        
        public int cols;        
        public IntPtr data;     
        public IntPtr datastart;
        public IntPtr dataend;  
        public IntPtr datalimit;
        public IntPtr step_p;   
        public IntPtr refcount; 


        private const int CV_MAT_CONT_FLAG = 0x4000;     
        private const int CV_MAT_TYPE_MASK = 0x00000FFF; 

        public int Width => cols;

        public int Height => rows;

        public int Type
        {
            get => flags & CV_MAT_TYPE_MASK;
        }

        public int Depth
        {
            get => Type & 7;
        }

        public int Channels
        {
            get => ((Type >> 3) & 511) + 1;
        }

        public int BytesPerChannel
        {
            get => GetBytesPerChannel(Depth);
        }

        public int BytesPerPixel
        {
            get => Channels * BytesPerChannel;
        }

        public int ExpectedStep
        {
            get => cols * BytesPerPixel;
        }

        public int ActualStep
        {
            get
            {
                if (step_p != IntPtr.Zero)
                {
                    try
                    {

                        return Marshal.ReadIntPtr(step_p).ToInt32();
                    }
                    catch
                    {
                        return ExpectedStep;
                    }
                }
                return ExpectedStep;
            }
        }

        public bool IsContinuous
        {
            get
            {
                bool flagsContinuous = (flags & CV_MAT_CONT_FLAG) != 0;

                bool stepContinuous = ActualStep == ExpectedStep;

                bool pointerRangeContinuous = CheckPointerRange();

                return flagsContinuous && stepContinuous;
            }
        }

        private bool CheckPointerRange()
        {
            try
            {
                if (data == IntPtr.Zero || dataend == IntPtr.Zero || datastart == IntPtr.Zero)
                    return false;

                long expectedTotalBytes = (long)rows * ExpectedStep;
                
                long actualDataRange = dataend.ToInt64() - data.ToInt64();

                return Math.Abs(actualDataRange - expectedTotalBytes) <= ActualStep;
            }
            catch
            {
                return true; 
            }
        }

        public int GetBytesPerChannel(int depth)
        {
            switch (depth)
            {
                case 0: return 1; 
                case 1: return 1; 
                case 2: return 2; 
                case 3: return 2; 
                case 4: return 4; 
                case 5: return 4; 
                case 6: return 8; 
                case 7: return 2; 
                default: return 1;
            }
        }

        public string TypeDescription
        {
            get
            {
                string depthName;
                switch (Depth)
                {
                    case 0: depthName = "CV_8U"; break;
                    case 1: depthName = "CV_8S"; break;
                    case 2: depthName = "CV_16U"; break;
                    case 3: depthName = "CV_16S"; break;
                    case 4: depthName = "CV_32S"; break;
                    case 5: depthName = "CV_32F"; break;
                    case 6: depthName = "CV_64F"; break;
                    case 7: depthName = "CV_16F"; break;
                    default: depthName = $"Unknown({Depth})"; break;
                }
                return $"{depthName}C{Channels}";
            }
        }

        public string ContinuityInfo
        {
            get
            {
                var info = new System.Text.StringBuilder();
                info.AppendLine($"Type: {TypeDescription}");
                info.AppendLine($"Size: {cols}x{rows}");
                info.AppendLine($"Flags Continuous: {((flags & CV_MAT_CONT_FLAG) != 0)}");
                info.AppendLine($"Expected Step: {ExpectedStep}");
                info.AppendLine($"Actual Step: {ActualStep}");
                info.AppendLine($"Step Match: {ActualStep == ExpectedStep}");
                info.AppendLine($"Final Continuity: {IsContinuous}");
                return info.ToString();
            }
        }

        public byte[] ToContiguousBytes()
        {
            if (data == IntPtr.Zero || rows <= 0 || cols <= 0)
                return new byte[0];

            int expectedStep = ExpectedStep;
            int actualStep = ActualStep;

            if (IsContinuous)
            {
                int totalBytes = rows * expectedStep;
                byte[] continuousData = new byte[totalBytes];
                Marshal.Copy(data, continuousData, 0, totalBytes);
                return continuousData;
            }
            else
            {
                byte[] continuousData = new byte[rows * expectedStep];

                for (int row = 0; row < rows; row++)
                {
                    IntPtr srcRowPtr = new IntPtr(data.ToInt64() + (long)row * actualStep);
                    int dstOffset = row * expectedStep;

                    byte[] rowData = new byte[expectedStep];
                    Marshal.Copy(srcRowPtr, rowData, 0, expectedStep);
                    Array.Copy(rowData, 0, continuousData, dstOffset, expectedStep);
                }

                return continuousData;
            }
        }

      
    }
}
