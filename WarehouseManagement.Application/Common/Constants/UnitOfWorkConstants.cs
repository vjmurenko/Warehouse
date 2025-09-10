namespace WarehouseManagement.Application.Common.Constants;

public static class UnitOfWorkConstants
{
    public const string TransactionAlreadyStartedError = "A transaction has already been started";
    public const string NoActiveTransactionError = "No active transaction to commit";
    public const string TransactionCommitError = "Failed to commit transaction";
    public const string TransactionRollbackError = "Failed to rollback transaction";
}