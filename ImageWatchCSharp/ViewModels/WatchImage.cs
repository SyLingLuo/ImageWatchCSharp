using EnvDTE;
using EnvDTE80;
using ImageWatchCSharp.Bind;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Interop;

namespace ImageWatchCSharp.ViewModels
{
    public class WatchImage : BindableBase, IDisposable
    {
        private DTE2 dte;

        private IntPtr _hProcess;
        private int _pid = -1;

        private NativeImage nativeImage;
        private bool _isDisposed; // 新增字段，标识Mat对象是否被释放

        private IntPtr _lastPtr = IntPtr.Zero;
        private int _lastWidth = -1;
        private int _lastHeight = -1;
        private int _lastChannels = -1;
        private int _lastTypeValue = -1;

        public string VarName { get; set; }

        /// <summary>
        /// 变量类型描述字符串
        /// </summary>
        public string VarType { get; set; }
        public string Expression { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// 表示当前Mat对象是否已被释放
        /// </summary>
        public bool IsDisposed
        {
            get => _isDisposed;
            private set
            {
                if (_isDisposed != value)
                {
                    _isDisposed = value;
                    NotifyPropertyChanged(nameof(IsDisposed));
                }
            }
        }

        public int Width
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return nativeImage.Cols;
            }
        }

        public int Height
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return nativeImage.Rows;
            }
        }

        public int Channels
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return nativeImage.Channels;
            }
        }

        public MatType Type
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return nativeImage.Type;
            }
        }

        public InteropBitmap Image
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return nativeImage.Image;
            }
        }

        public WatchImage(Expression expression)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.dte = DebuggerMonitor.Instance.DTE;
            this.Expression = expression.Name;
            this.VarName = expression.Name;
            //获取该变量的类型信息
            this.VarType = expression.Type;

            IntPtr handle = DebuggerMonitor.Instance.ProcessHandle;
            if (!CheckExpressionValid(expression))
            {
                nativeImage = new NativeImage(handle, IntPtr.Zero);
                IsDisposed = true; 
                return;
            }
            string strPtr = expression.DataMembers?.Item("CvPtr")?.Value;
            var ptr = ParsePointer(strPtr);
            nativeImage = new NativeImage(handle, ptr);
            IsDisposed = false; 
        }

        public string GetImagePixel(int x, int y) => nativeImage.GetOriginalPixelValueAtPoint(x, y);


        public void Refresh()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (dte?.Debugger?.CurrentMode != dbgDebugMode.dbgBreakMode)
                return;

            try
            {
                var expression = dte.Debugger.GetExpression(this.Expression);

                bool isDisposed = false;

                if (!expression.IsValidValue)
                {
                    isDisposed = true;
                }
                else if (expression.Value != null &&
                        (expression.Value.Contains("<disposed>") ||
                         expression.Value.Contains("已释放") ||
                         expression.Value.Contains("disposed")))
                {
                    isDisposed = true;
                }
                else if (expression.DataMembers == null)
                {
                    isDisposed = true;
                }
                else
                {
                    try
                    {
                        var cvPtrMember = expression.DataMembers.Item("CvPtr");
                        if (cvPtrMember == null)
                        {
                            isDisposed = true;
                        }
                        else
                        {
                            string strPtr = cvPtrMember.Value;
                            var ptr = ParsePointer(strPtr);
                            if (ptr == IntPtr.Zero)
                            {
                                isDisposed = true;
                            }
                            else
                            {
                                IntPtr handle = DebuggerMonitor.Instance.ProcessHandle;

                                bool needsUpdate = false;

                                if (ptr != _lastPtr || 
                                    nativeImage.Cols != _lastWidth || 
                                    nativeImage.Rows != _lastHeight || 
                                    nativeImage.Channels != _lastChannels ||
                                    nativeImage.Type.Value != _lastTypeValue)
                                {
                                    needsUpdate = true;
                                    
                                    _lastPtr = ptr;
                                    _lastWidth = nativeImage.Cols;
                                    _lastHeight = nativeImage.Rows;
                                    _lastChannels = nativeImage.Channels;
                                    _lastTypeValue = nativeImage.Type.Value;
                                    
                                    nativeImage?.Dispose();
                                    nativeImage = new NativeImage(handle, ptr);
                                    
                                }

                                IsDisposed = false;

                                if (needsUpdate)
                                {
                                    NotifyPropertyChanged(nameof(Width));
                                    NotifyPropertyChanged(nameof(Height));
                                    NotifyPropertyChanged(nameof(Channels));
                                    NotifyPropertyChanged(nameof(Type));
                                    NotifyPropertyChanged(nameof(Image));
                                }
                                return; 
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        isDisposed = true;
                    }
                }

                if (isDisposed)
                {
                    nativeImage?.Dispose();
                    nativeImage = new NativeImage(DebuggerMonitor.Instance.ProcessHandle, IntPtr.Zero);

                    IsDisposed = true;

                    NotifyPropertyChanged(nameof(Image));
                }
            }
            catch (Exception ex)
            {
                nativeImage?.Dispose();
                nativeImage = new NativeImage(DebuggerMonitor.Instance.ProcessHandle, IntPtr.Zero);
                IsDisposed = true;
                NotifyPropertyChanged(nameof(Image));
            }
        }

        private IntPtr ParsePointer(string s)
        {
            if (string.IsNullOrEmpty(s)) return IntPtr.Zero;

            var hexMatch = Regex.Match(s, @"0x[0-9a-fA-F]+");
            if (hexMatch.Success)
            {
                if (long.TryParse(hexMatch.Value.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out long v))
                    return new IntPtr(v);
            }

            var decMatch = Regex.Match(s, @"\d+");
            if (decMatch.Success)
            {
                if (long.TryParse(decMatch.Value, out long v))
                    return new IntPtr(v);
            }
            return IntPtr.Zero;
        }

        private bool CheckExpressionValid(Expression expression)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (expression == null ||
                string.IsNullOrEmpty(expression.Value) ||
                expression.Value.Contains("<undefined>") ||
                expression.Value.Contains("<未初始化>") ||
                expression.Value.Contains("<Unable to read memory>") ||
                expression.Value == "null")
                return false;
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WatchImage()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                nativeImage?.Dispose();
            }
        }
    }
}