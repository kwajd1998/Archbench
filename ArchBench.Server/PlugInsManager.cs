using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArchBench.PlugIns;

namespace ArchBench.Server
{
    /// <summary>
    /// Summary description for PlugInsManager.
    /// </summary>
    public class PlugInsManager : IPlugInsManager
    {
        #region Fields
        private readonly IList<IArchBenchPlugIn> mPlugIns = new List<IArchBenchPlugIn>();
        #endregion

        public PlugInsManager( IArchBenchPlugInHost aHost )
        {
            Host = aHost;
        }

        public IArchBenchPlugInHost Host { get; }

        public IEnumerable<IArchBenchPlugIn> PlugIns => mPlugIns;

        public IEnumerable<IArchBenchPlugIn> Add( string aFileName )
        {
            // Create a new assembly from the plugin file we're adding..
            Assembly assembly = Assembly.LoadFrom( aFileName );

            var instances = new List<IArchBenchPlugIn>();

            // Next we'll loop through all the Types found in the assembly
            foreach ( var type in assembly.GetTypes() )
            {
                if ( ! type.IsPublic ) continue;
                if ( type.IsAbstract ) continue;

                // Gets a type object of the interface we need the plugins to match
                Type typeInterface = type.GetInterface( $"ArchBench.PlugIns.{ nameof( IArchBenchPlugIn ) }", true );

                // Make sure the interface we want to use actually exists
                if ( typeInterface == null ) continue;

                // Create a new instance and store the instance in the collection for later use
                var instance = (IArchBenchPlugIn) Activator.CreateInstance( assembly.GetType( type.ToString() ) );

                // Set the Plug-in's host to this class which inherited IPluginHost
                instance.Host = Host;

                // Call the initialization sub of the plugin
                instance.Initialize();

                //Add the new plugin to our collection here
                mPlugIns.Add( instance );

                instances.Add( instance );
            }

            return instances;
        }

        public void Remove( IArchBenchPlugIn aPlugIn )
        {
            if ( mPlugIns.Contains( aPlugIn ) ) mPlugIns.Remove( aPlugIn );
        }

        public IArchBenchPlugIn Find( string aName )
        {
            return mPlugIns.FirstOrDefault( p => p.Name == aName );
        }

        public void Clear()
        {
            foreach ( var plugin in mPlugIns )
            {
                // Close all plugin instances
                plugin.Dispose();
            }

            //Finally, clear our collection of available plugins
            mPlugIns.Clear();
        }
    }
}