using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Net;
using ArchBench.PlugIns;

namespace ArchBench.Server
{
    public partial class ArchServerForm : Form
    {

        public ArchServerForm()
        {
            InitializeComponent();
            Logger = new TextBoxLogger( mOutput );
            ModulePugIns = new ModulePlugIns( Logger );
        }

        public HttpServer.HttpServer Server { get; private set; }
        private ModulePlugIns        ModulePugIns { get; }
        private IArchBenchLogger     Logger { get; }

        #region Toolbar Double Click problem

        private bool HandleFirstClick { get; set; } = false;

        protected override void OnActivated( EventArgs e )
        {
            base.OnActivated( e );
            if (HandleFirstClick)
            {
                var position = Cursor.Position;
                var point = this.PointToClient(position);
                var child = this.GetChildAtPoint(point);
                while ( HandleFirstClick && child != null )
                {
                    if (child is ToolStrip toolStrip)
                    {
                        HandleFirstClick = false;
                        point = toolStrip.PointToClient(position);
                        foreach (var item in toolStrip.Items)
                        {
                            if (item is ToolStripItem toolStripItem && toolStripItem.Bounds.Contains(point))
                            {
                                if (item is ToolStripMenuItem tsMenuItem)
                                {
                                    tsMenuItem.ShowDropDown();
                                }
                                else
                                {
                                    toolStripItem.PerformClick();
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        child = child.GetChildAtPoint(point);
                    }
                }
                HandleFirstClick = false;
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_ACTIVATE = 0x0006;
            const int WA_CLICKACTIVE = 0x0002;
            if (m.Msg == WM_ACTIVATE && Low16(m.WParam) == WA_CLICKACTIVE)
            {
                HandleFirstClick = true;
            }
            base.WndProc( ref m );
        }

        private static int GetIntUnchecked(IntPtr value)
        {
            return IntPtr.Size == 8 ? unchecked((int)value.ToInt64()) : value.ToInt32();
        }

        private static int Low16(IntPtr value)
        {
            return unchecked((short)GetIntUnchecked(value));
        }

        private static int High16(IntPtr value)
        {
            return unchecked((short)(((uint)GetIntUnchecked(value)) >> 16));
        }

        #endregion
        
        private void OnExit(object sender, EventArgs e)
        {
            Server?.Stop();
            ModulePugIns.Manager.Clear();
            Application.Exit();
        }

        private void OnConnect(object sender, EventArgs e)
        {
            mConnectTool.Checked = ! mConnectTool.Checked;
            if ( mConnectTool.Checked )
            {
                Server = new HttpServer.HttpServer();
                Server.Add( ModulePugIns );
                Server.Start( IPAddress.Any, int.Parse( mPort.Text ) );
                Logger.WriteLine( "Server online on port {0}", mPort.Text );

                mConnectTool.Image = Properties.Resources.connect;
            }
            else
            {
                Server.Stop();
                Server = null;

                mConnectTool.Image = Properties.Resources.disconnect;
            }
        }

        private void OnPlugIn( object sender, EventArgs e )
        {
            var dialog = new PlugInsForm( ModulePugIns.Manager );
            dialog.ShowDialog();
        }
    }
}
