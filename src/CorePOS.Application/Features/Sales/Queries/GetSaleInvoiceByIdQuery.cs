using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Sales.DTOs;

namespace CorePOS.Application.Features.Sales.Queries;

public record GetSaleInvoiceByIdQuery(int Id)  : IRequest<Result<SalesInvoiceDto>>;
public record GetSaleInvoiceByNoQuery(string InvoiceNo) : IRequest<Result<SalesInvoiceDto>>;
