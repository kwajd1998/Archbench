using System;
using System.Drawing;
using System.Windows.Forms;
using ArchBench.PlugIns;

namespace ArchBench.Server
{
    public partial class PlugInsForm : Form
    {
        public PlugInsForm( IPlugInsManager aManager )
        {
            InitializeComponent();
            mPlugInsListView.SelectedIndexChanged +=
                ( sender, args ) => mSettingsButton.Enabled = mPlugInsListView.SelectedItems.Count > 0;
            mPlugInsListView.SelectedIndexChanged +=
                ( sender, args ) => mRemoveButton.Enabled = mPlugInsListView.SelectedItems.Count > 0;

            PlugInsManager = aManager;

            foreach ( var plugin in PlugInsManager.PlugIns )
            {
                Append( plugin );
            }
        }

        private IPlugInsManager PlugInsManager { get; set; }

        private void Append(IArchBenchPlugIn aPlugIn)
        {
            var item = new ListViewItem {
                Text       = aPlugIn.Name,
                Checked    = aPlugIn.Enabled,
                ImageIndex = 0,
                Tag        = aPlugIn
            };

            item.SubItems.Add( aPlugIn.Version );
            item.SubItems.Add( aPlugIn.Author );
            item.SubItems.Add( aPlugIn.Description );

            mPlugInsListView.Items.Add( item );
        }

        private void OnAppend( object sender, EventArgs e )
        {
            var dialog = new OpenFileDialog() { Multiselect = true };
            dialog.Filter = @"Arch.Bench PlugIn File (*.dll)|*.dll";

            if ( dialog.ShowDialog() == DialogResult.OK )
            {
                foreach ( var name in dialog.FileNames )
                {
                    var plugins = PlugInsManager.Add( name );
                    foreach ( var plugin in plugins )
                    {
                        Append( plugin );
                    }
                }
            }
        }

        private void OnRemove( object sender, EventArgs e )
        {
            foreach ( ListViewItem item in mPlugInsListView.SelectedItems )
            {
                var plugin = (IArchBenchPlugIn) item.Tag;
                if ( plugin == null ) continue;

                PlugInsManager.Remove( plugin );
                item.Remove();
            }
        }

        private void OnItemChecked( object sender, ItemCheckedEventArgs e )
        {
            e.Item.ImageIndex = e.Item.Checked ? 0 : 1;
            e.Item.ForeColor = e.Item.Checked ? Color.Empty : Color.Gray;

            var plugin = (IArchBenchPlugIn) e.Item.Tag;
            if ( plugin != null ) plugin.Enabled = e.Item.Checked;
        }

        private void OnSettings( object sender, EventArgs e )
        {
            if ( mPlugInsListView.SelectedItems.Count == 0 ) return;

            var dialog = new PlugInsSettingsForm( mPlugInsListView.SelectedItems[0].Tag as IArchBenchPlugIn );
            dialog.ShowDialog();
        }
    }
}
