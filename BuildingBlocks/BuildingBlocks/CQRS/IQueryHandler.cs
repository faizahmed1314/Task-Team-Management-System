using MediatR;

namespace BuildingBlocks.CQRS
{
    public interface IQueryHandler<in TQuery, TRespnose> : IRequestHandler<TQuery, TRespnose>
        where TQuery : IQuery<TRespnose>
        where TRespnose : notnull
    {
    }
}