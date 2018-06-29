using Microsoft.VisualStudio.Shell.TableManager;

namespace VHDL_Extension.Errors
{
    class VhdlErrorsFactory : TableEntriesSnapshotFactoryBase
    {
        private readonly VhdlChecker _spellChecker;

        public VhdlErrorsSnapshot CurrentSnapshot { get; private set; }

        public VhdlErrorsFactory(VhdlChecker spellChecker, VhdlErrorsSnapshot vhdlErrors)
        {
            _spellChecker = spellChecker;

            this.CurrentSnapshot = vhdlErrors;
        }

        internal void UpdateErrors(VhdlErrorsSnapshot spellingErrors)
        {
            this.CurrentSnapshot.NextSnapshot = spellingErrors;
            this.CurrentSnapshot = spellingErrors;
        }

        #region ITableEntriesSnapshotFactory members
        public override int CurrentVersionNumber => CurrentSnapshot.VersionNumber;

        public override void Dispose()
        {
        }

        public override ITableEntriesSnapshot GetCurrentSnapshot()
        {
            return this.CurrentSnapshot;
        }

        public override ITableEntriesSnapshot GetSnapshot(int versionNumber)
        {
            // In theory the snapshot could change in the middle of the return statement so snap the snapshot just to be safe.
            var snapshot = this.CurrentSnapshot;
            return (versionNumber == snapshot.VersionNumber) ? snapshot : null;
        }
        #endregion
    }
}
