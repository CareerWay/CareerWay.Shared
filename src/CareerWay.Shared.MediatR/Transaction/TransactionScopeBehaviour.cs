﻿using MediatR;
using System.Transactions;

namespace CareerWay.Shared.MediatR.Transaction;

public class TransactionScopeBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactionalRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
        TResponse response;
        try
        {
            response = await next();
            transactionScope.Complete();
        }
        catch (Exception)
        {
            transactionScope.Dispose();
            throw;
        }

        return response;
    }
}
