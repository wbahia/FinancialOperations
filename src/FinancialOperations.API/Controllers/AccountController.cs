using FinancialOperations.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace FinancialOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ProcessCreditUseCase _processCreditUseCase;
    private readonly ProcessDebitUseCase _processDebitUseCase;
    private readonly ProcessTransferUseCase _processTransferUseCase;
    private readonly ProcessReserveUseCase _processReserveUseCase;

    public AccountsController(
        ProcessCreditUseCase processCreditUseCase,
        ProcessDebitUseCase processDebitUseCase,
        ProcessTransferUseCase processTransferUseCase,
        ProcessReserveUseCase processReserveUseCase)
    {
        _processCreditUseCase = processCreditUseCase;
        _processDebitUseCase = processDebitUseCase;
        _processTransferUseCase = processTransferUseCase;
        _processReserveUseCase = processReserveUseCase;
    }

    [HttpPost("{accountId}/credit")]
    public async Task<IActionResult> Credit(Guid accountId, [FromBody] CreditRequest request)
    {
        try
        {
            var creditRequest = new ProcessCreditRequest
            {
                AccountId = accountId,
                Amount = request.Amount,
                Description = request.Description
            };

            var result = await _processCreditUseCase.ExecuteAsync(creditRequest);
            return Ok(new { Success = result, Message = "Credito processado com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{accountId}/debit")]
    public async Task<IActionResult> Debit(Guid accountId, [FromBody] DebitRequest request)
    {
        try
        {
            var debitRequest = new ProcessDebitRequest
            {
                AccountId = accountId,
                Amount = request.Amount,
                Description = request.Description
            };

            var result = await _processDebitUseCase.ExecuteAsync(debitRequest);
            return Ok(new { Success = result, Message = "Debito processado com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{accountId}/reserve")]
    public async Task<IActionResult> Reserve(Guid accountId, [FromBody] ReserveRequest request)
    {
        try
        {
            var reserveRequest = new ProcessReserveRequest
            {
                AccountId = accountId,
                Amount = request.Amount,
                Description = request.Description
            };

            var result = await _processReserveUseCase.ExecuteAsync(reserveRequest);
            return Ok(new { Success = result, Message = "Reserva processada com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            var transferRequest = new ProcessTransferRequest
            {
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                Amount = request.Amount,
                Description = request.Description
            };

            var result = await _processTransferUseCase.ExecuteAsync(transferRequest);
            return Ok(new { Success = result, Message = "Transferencia processada com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}