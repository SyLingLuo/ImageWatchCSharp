using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ImageWatchCSharp.ToolWindow
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ViewMatCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0200;


        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private volatile bool forcedEnabled; // 可被 DebuggerMonitor 强制启用/禁用（可选）
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewMatCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ViewMatCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            forcedEnabled = true;
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            Guid guid = new Guid(GuidList.guidCodeMenuVSPackageCmdSet);
            CommandID menuCommandID2 = new CommandID(guid, CommandId);

            OleMenuCommand menuItem2 = new OleMenuCommand(Execute, menuCommandID2);
            menuItem2.BeforeQueryStatus += BeforeQueryStatus;
            if (menuItem2 is OleMenuCommand oleMenuCmd)
            {
                try
                {
                    var sortField = typeof(OleMenuCommand).GetField("OLECMDF_DEFHIDEONCTXTMENU",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (sortField != null)
                        sortField.SetValue(oleMenuCmd, -1);
                }
                catch { }
            }

            commandService.AddCommand(menuItem2);

        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ViewMatCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ViewMatCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ViewMatCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private  async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // 打开 ToolWindow
            var window = await this.package.ShowToolWindowAsync(typeof(MainToolWindow), 0, true, this.package.DisposalToken);
            if (window == null || window.Frame == null)
                throw new NotSupportedException("Cannot create tool window");
        }

        public void SetEnabled(bool enabled)
        {
            // 允许外部（如 DebuggerMonitor）粗粒度控制开关
            forcedEnabled = enabled;
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            System.Diagnostics.Debug.WriteLine("ViewMatCommand.BeforeQueryStatus");
            var cmd = (MenuCommand)sender;


            var dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
            if (dte == null) return;

            if (dte.Debugger?.CurrentMode != dbgDebugMode.dbgBreakMode)
                return;

            // 取编辑器中当前选中文本或光标处单词作为表达式
            var exprText = GetExpressionUnderCaret(dte);
            if (string.IsNullOrWhiteSpace(exprText))
                return;

            try
            {
                Expression expr = dte.Debugger.GetExpression(exprText.Trim(), true, 1);
                if (expr != null && expr.IsValidValue)
                {
                    var typeName = expr.Type ?? string.Empty;
                    // 识别 OpenCvSharp.Mat 或末尾为 .Mat / Mat（可按需收紧）
                    bool isMat = typeName.Equals("OpenCvSharp.Mat", StringComparison.Ordinal)
                                 || typeName.EndsWith(".Mat", StringComparison.Ordinal)
                                 || typeName.Equals("Mat", StringComparison.Ordinal);

                    if (isMat)
                    {
                        cmd.Visible = true;
                        cmd.Enabled = forcedEnabled;
                    }
                    else
                    {
                        cmd.Visible = false;
                        cmd.Enabled = false;
                    }
                }
            }
            catch
            {
                
            }
        }
        private static string GetExpressionUnderCaret(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var doc = dte.ActiveDocument;
            if (doc == null) return null;

            var sel = doc.Selection as TextSelection;
            if (sel == null) return null;

            // 优先使用已有选择
            var selected = sel.Text;
            if (!string.IsNullOrWhiteSpace(selected))
                return selected;

            // 无选择：从当前行中根据光标位置提取“单词”（字母数字、下划线、点、->、[] 等粗略支持）
            int line = sel.ActivePoint.Line;
            var ep = sel.ActivePoint.CreateEditPoint();
            string lineText = ep.GetLines(line, line + 1);
            if (string.IsNullOrEmpty(lineText)) return null;

            int col = Math.Max(1, sel.ActivePoint.LineCharOffset) - 1;
            col = Math.Min(col, Math.Max(0, lineText.Length - 1));

            int left = col, right = col;
            Func<char, bool> isExprChar = ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '.' || ch == '>' || ch == '-' || ch == '[' || ch == ']' || ch == '(' || ch == ')';

            while (left > 0 && isExprChar(lineText[left - 1])) left--;
            while (right < lineText.Length && isExprChar(lineText[right])) right++;

            string word = lineText.Substring(left, Math.Max(0, right - left)).Trim();
            // 去掉可能的括号/索引尾巴
            if (word.EndsWith(")")) word = word.TrimEnd(')');
            return string.IsNullOrWhiteSpace(word) ? null : word;
        }
    }
}
