using System;
using System.Runtime.InteropServices;

namespace ImageWatchCSharp
{

    public enum PseudoColorMode
    {
        GRAY = 0,
        JET = 1,
        HOT = 2,
        COOL = 3,
        HSV = 4,
        RAINBOW = 5,
        CUSTOM = 6
    }

    internal static class PseudoColorMapperInterop
    {
        private const string DllName = "Framework.AlgorithmDll.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_pseudo_color_mapper(int mode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void delete_pseudo_color_mapper(IntPtr mapper);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_color_mode(IntPtr mapper, int mode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int apply_pseudo_color(IntPtr mapper, 
            IntPtr input_data, int input_width, int input_height, int input_channels,
            IntPtr output_data, int target_channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int get_color_bar(IntPtr mapper, 
            IntPtr output_data, int width, int height);
            
        public static bool ReleaseMapper(ref IntPtr mapper)
        {
            if (mapper == IntPtr.Zero)
                return false;
                
            try
            {
                delete_pseudo_color_mapper(mapper);
                mapper = IntPtr.Zero;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public static PseudoColorMapperWrapper CreateMapper(PseudoColorMode mode)
        {
            IntPtr handle = create_pseudo_color_mapper((int)mode);
            if (handle == IntPtr.Zero)
                return null;
                
            return new PseudoColorMapperWrapper(handle);
        }
    }
    
    internal class PseudoColorMapperWrapper : IDisposable
    {
        private IntPtr _handle;
        private bool _disposed = false;
        
        public IntPtr Handle => _handle;
        
        internal PseudoColorMapperWrapper(IntPtr handle)
        {
            _handle = handle;
        }
        
        public void SetColorMode(PseudoColorMode mode)
        {
            ThrowIfDisposed();
            PseudoColorMapperInterop.set_color_mode(_handle, (int)mode);
        }
        
        public bool ApplyPseudoColor(
            IntPtr inputData, int inputWidth, int inputHeight, int inputChannels,
            IntPtr outputData, int targetChannel = -1)
        {
            ThrowIfDisposed();
            return PseudoColorMapperInterop.apply_pseudo_color(
                _handle, inputData, inputWidth, inputHeight, inputChannels, 
                outputData, targetChannel) != 0;
        }
        
        public bool GetColorBar(IntPtr outputData, int width, int height)
        {
            ThrowIfDisposed();
            return PseudoColorMapperInterop.get_color_bar(_handle, outputData, width, height) != 0;
        }
        
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PseudoColorMapperWrapper));
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
                if (_handle != IntPtr.Zero)
                {
                    PseudoColorMapperInterop.delete_pseudo_color_mapper(_handle);
                    _handle = IntPtr.Zero;
                }
                _disposed = true;
            }
        }
        
        ~PseudoColorMapperWrapper()
        {
            Dispose(false);
        }
    }
}