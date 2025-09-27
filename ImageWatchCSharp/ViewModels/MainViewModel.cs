using EnvDTE;
using EnvDTE80;
using ImageWatchCSharp.Bind;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ImageWatchCSharp.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private DTE2 dte;
        private WatchImage _selectedWatchImage;
        private string _currentPixelInfo;

        public ObservableCollection<WatchImage> StackImages { get; set; }
        public ObservableCollection<WatchImage> WatchImages { get; set; }

        public ObservableCollection<Expression> Expressions { get; set; }

        public WatchImage SelectedWatchImage
        {
            get { return _selectedWatchImage; }
            set
            {
                if (SetProperty(ref _selectedWatchImage, value))
                {
                      _selectedWatchImage?.Refresh();
                }
            }
        }

        public string CurrentPixelInfo
        {
            get { return _currentPixelInfo; }
            set { SetProperty(ref _currentPixelInfo, value); }
        }

        public MainViewModel(DTE2 dte)
        {
            this.dte = dte;
            Initialize();
        }

        private void Initialize()
        {
            StackImages = new ObservableCollection<WatchImage>();
            WatchImages = new ObservableCollection<WatchImage>();
            Expressions = new ObservableCollection<Expression>();
            InitializeStackImages();
        }

        private void InitializeStackImages()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Debugger debugger = dte.Debugger;
            List<string> list = new List<string>();
            if (debugger.CurrentStackFrame is null)
                return;
            FindExpressionInStack(debugger.CurrentStackFrame);
        }

        private void FindExpressionInStack(StackFrame stack)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (stack is null)
            {
                ClearStackImages();
                return;
            }

            var currentMatVariables = new HashSet<string>();
            
            foreach (object obj in stack.Locals)
            {
                Expression expr = (Expression)obj;
                if (expr.IsValidValue && expr.Type == "OpenCvSharp.Mat"  && !expr.Name.Contains("OpenCvSharp.MatExpr.implicit operator"))
                {
                    currentMatVariables.Add(expr.Name);
                    
                    if (ExpressionExistsInStackImages(expr.Name))
                    {
                        WatchImage image = GetWatchImageFromStackImages(expr.Name);
                        image?.Refresh();
                    }
                    else
                    {
                        var watchImage = new WatchImage(expr);
                        StackImages.Add(watchImage);
                        if (!ExpressionExists(expr.Name))
                        {
                            Expressions.Add(expr);
                        }
                    }
                }
            }
            RemoveObsoleteStackImages(currentMatVariables);
        }


        private bool ExpressionExistsInStackImages(string variableName)
        {
            return StackImages.Any(img => img.VarName == variableName);
        }


        private WatchImage GetWatchImageFromStackImages(string variableName)
        {
            return StackImages.FirstOrDefault(img => img.VarName == variableName);
        }


        private void RemoveObsoleteStackImages(HashSet<string> currentMatVariables)
        {
            var imagesToRemove = StackImages.Where(img => !currentMatVariables.Contains(img.VarName)).ToList();
            
            foreach (var imageToRemove in imagesToRemove)
            {
                if (SelectedWatchImage == imageToRemove)
                {
                    SelectedWatchImage = null;
                }
                
                imageToRemove.Dispose();
                
                StackImages.Remove(imageToRemove);
                
                var exprToRemove = Expressions.FirstOrDefault(expr => expr.Name == imageToRemove.VarName);
                if (exprToRemove != null)
                {
                    Expressions.Remove(exprToRemove);
                }
            }
        }


        private void ClearStackImages()
        {
            SelectedWatchImage = null;
            
            foreach (var image in StackImages)
            {
                image.Dispose();
            }
            
            StackImages.Clear();
            Expressions.Clear();
        }

        private bool ExpressionExists(string expression)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var img in Expressions)
            {
                if (img.Name == expression)
                    return true;
            }
            return false;
        }


 
        public void RefreshImages()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            FindExpressionInStack(dte.Debugger.CurrentStackFrame);
        }
    }
}
