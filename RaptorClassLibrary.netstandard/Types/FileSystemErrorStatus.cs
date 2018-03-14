﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// The result coniditons returnable by the Raptor persistence layer that implements FS files.
    /// </summary>
    public enum FileSystemErrorStatus
    {
        OK,
        FileSystemNotFound,
        FileSystemAlreadyExists,
        FileSystemRecoveryFailure,
        ErrorReadingFreeSpaceList,
        ErrorReadingSpatialSubgridIndex,
        ErrorConstructingSpatialSubgridExistanceMap,
        FileSystemCorrupt,
        ErrorReadingFileList,
        UnknownErrorCreatingFS,
        UnknownErrorOpeningFS,
        UnknownErrorWritingToFS,
        UnknownErrorWritingToFSCache,
        UnknownErrorReadingFromFS,
        UnknownErrorReadingFromFSCache,
        UnknownErrorExpandingFS,
        UnknownErrorShrinkingFS,
        UnknownErrorClosingFS,
        UnknownFailureRemovingFileFromFS,
        UnknownFailureRemovingFileFromFSCache,
        UnknownErrorExplodingFS,
        UnknownErrorFindingFile,
        ErrorOpeningFile,
        ErrorClosingFile,
        FileNotAvailableForReading,
        FileNotAvailableForWriting,
        FileDoesNotExist,
        FileSeekFailure,
        FileReadFailure,
        FileReadInsufficientBytes,
        FileWriteInsufficientBytes,
        FileWriteFailure,
        FileCRCFailure,
        FileSizeExtensionFailure,
        FileSizeTruncationFailure,
        GranuleSerialiseInFailure,
        GranuleSerialiseOutFailure,
        GranuleDoesNotExist,
        DuplicateItemInOutstandingAllocationsList,
        FileToReadNotSafelyCommitted,
        OutOfMemoryReadingFile,
        OutOfMemoryReadingFileFromCache,
        UnableToLockMemoryForReading,
        UnableToUnLockMemoryForReading,
        OutOfMemoryWritingFile,
        UnableToLockMemoryForWriting,
        UnableToUnLockMemoryForWriting,
        UnableFreeMemoryForWriting,
        UnableToLocateFreeSpaceToStoreFile,
        FileSystemOpenedAsReadOnly,
        ErrorOptimizingFile,
        NoActiveTransaction,
        TransactionAlreadyActive,
        ErrorUpdatingGroupTransactionID,
        ErrorStartingTransaction,
        ErrorCompletingTransaction,
        FileSystemRequiresUpgrade,
        TimeoutReadingFromFS,
        TimeoutWritingToFS,
        SpatialStreamIndexGranuleLocationNull,
        IOServerConnectionFailure,
        DataModelIDMismatch,
        ServiceStopped,
        UnableToAcquireGranuleReadInterlock,
        FailedToCreateAllConcurrentHandles,
        FailedToLockFileRegion,
        InvalidRequest
    }
}
