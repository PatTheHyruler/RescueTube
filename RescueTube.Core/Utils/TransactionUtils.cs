using System.Transactions;

namespace RescueTube.Core.Utils;

public static class TransactionUtils
{
    public static TransactionScope NewTransactionScope(
        TransactionScopeOption transactionScopeOption = TransactionScopeOption.RequiresNew
    )
    {
        return new TransactionScope(transactionScopeOption, TransactionScopeAsyncFlowOption.Enabled);
    }
}