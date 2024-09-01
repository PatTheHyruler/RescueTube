using System.Transactions;

namespace RescueTube.Core.Utils;

public static class TransactionUtils
{
    private static readonly TransactionOptions TransactionOptions =
        new() { IsolationLevel = IsolationLevel.ReadCommitted };

    public static TransactionScope NewTransactionScope(
        TransactionScopeOption transactionScopeOption = TransactionScopeOption.RequiresNew
    )
    {
        return new TransactionScope(
            transactionScopeOption, TransactionOptions, TransactionScopeAsyncFlowOption.Enabled);
    }
}