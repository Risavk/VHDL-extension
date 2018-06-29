using System;
using Microsoft.VisualStudio.Shell.TableManager;

namespace VHDL_Extension.Errors
{
    /// <summary>
    /// Every consumer of data from an <see cref="ITableDataSource"/> provides an <see cref="ITableDataSink"/> to record the changes. We give the consumer
    /// an IDisposable (this object) that they hang on to as long as they are interested in our data (and they Dispose() of it when they are done).
    /// </summary>
    class VhdlSinkManager : IDisposable
    {
        private readonly VhdlCheckerProvider _vhdlErrorsProvider;
        private readonly ITableDataSink _sink;

        internal VhdlSinkManager(VhdlCheckerProvider vhdlErrorsProvider, ITableDataSink sink)
        {
            _vhdlErrorsProvider = vhdlErrorsProvider;
            _sink = sink;

            vhdlErrorsProvider.AddSinkManager(this);
        }

        public void Dispose()
        {
            // Called when the person who subscribed to the data source disposes of the cookie (== this object) they were given.
            _vhdlErrorsProvider.RemoveSinkManager(this);
        }

        internal void AddSpellChecker(VhdlChecker vhdlChecker)
        {
            _sink.AddFactory(vhdlChecker.Factory);
        }

        internal void RemoveSpellChecker(VhdlChecker vhdlChecker)
        {
            _sink.RemoveFactory(vhdlChecker.Factory);
        }

        internal void UpdateSink()
        {
            _sink.FactorySnapshotChanged(null);
        }
    }
}
