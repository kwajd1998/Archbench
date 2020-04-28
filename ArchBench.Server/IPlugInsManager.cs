using System.Collections.Generic;
using ArchBench.PlugIns;

namespace ArchBench.Server
{
    public interface IPlugInsManager
    {
        /// <summary>
        /// A Collection of all plug-ins
        /// </summary>
        IEnumerable<IArchBenchPlugIn> PlugIns { get; }

        /// <summary>
        /// <summary>Loads all plug-ins contained in the specified file.</summary>
        /// </summary>
        /// <param name="aFileName">The assembly's path.</param>
        /// <returns>An enumeration of all new added plug-ins</returns>
        IEnumerable<IArchBenchPlugIn> Add( string aFileName );

        /// <summary>
        /// Removes the specified plug-in from the manager's collection. 
        /// </summary>
        /// <param name="aPlugIn">a reference to the plug-in</param>
        void Remove( IArchBenchPlugIn aPlugIn );

        /// <summary>
        /// Search for a plug-in
        /// </summary>
        /// <param name="aName">The name of the plug-in</param>
        /// <returns></returns>
        IArchBenchPlugIn Find( string aName );

        /// <summary>
        /// Unloads and Closes all plug-ins
        /// </summary>
        void Clear();
    }
}