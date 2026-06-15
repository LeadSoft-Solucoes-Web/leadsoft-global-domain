using LeadSoft.Adapter.BrasilAPI;
using LeadSoft.Adapter.BrasilAPI.DTOs.CNPJs;
using LeadSoft.Adapter.BrasilAPI.DTOs.Holidays;
using LeadSoft.Adapter.IBGE;
using LeadSoft.Adapter.IBGE.DTOs;
using LeadSoft.Adapter.ViaCep;
using LeadSoft.Adapter.ViaCep.DTOs;
using LeadSoft.Common.GlobalDomain.DTOs;
using LeadSoft.Common.GlobalDomain.Entities;
using LeadSoft.Common.Library;
using LeadSoft.Common.Library.Constants;
using LeadSoft.Common.Library.EnvUtils;
using LeadSoft.Common.Library.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;

using static LeadSoft.Common.GlobalDomain.Entities.Enums;
using static LeadSoft.Common.Library.Enumerators.Enums;

namespace LeadSoft.Common.GlobalDomain.Controllers;

/// <summary>
/// Default and abstract LeadSoft Base controller for common/private methods
/// </summary>
/// <remarks>
/// Useful information endpoints:
///  * API version
///  * Configuration info
///  * Brazil CEP (ZIP) address search
///  * Brazil States and Regions,
///  * Cities
///  * Months
///  * Brazil holidays
///  * Document (CPF/CNPJ) validations
/// </remarks>
[ApiController]
public abstract class LeadSoftRootController(Assembly aMainCallerAssembly) : ControllerBase
{

    /// <summary>
    /// Method available to retrieve current Web API version name
    /// </summary>
    /// <returns>DTO String Response</returns>
    [HttpGet("Version", Name = nameof(GetVersion))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<string> GetVersion()
        => Ok($"v{aMainCallerAssembly.GetName().Version}");

    /// <summary>
    /// Method available to retrieve current Web API environment name
    /// </summary>
    /// <returns>DTO String Response</returns>
    [HttpGet("Environment", Name = nameof(GetEnvironment))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<string> GetEnvironment()
        => Ok(EnvUtil.Get(EnvUtil.AspNet));

    /// <summary>
    /// Method that checks if JWT Authorization is valid or expired.
    /// </summary>
    /// <response code="200">You are authorized, your JWT seems to be valid.</response>
    /// <response code="401">You are unauthorized, <see langword="try"/> getting a new JTW.</response>
    [HttpGet("Authorization/Check", Name = nameof(GetValidAuthorization))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public IActionResult GetValidAuthorization()
        => Ok();

    /// <summary>
    /// Endpoint que disponbiliza chaves RSA
    /// </summary>
    /// <returns>Chaves pública e privada.</returns>
    [HttpGet("RSA", Name = nameof(GetRSAAsync))]
    [Produces(Constant.ApplicationProblemJson)]
    public async Task<IActionResult> GetRSAAsync()
    {
        RSA rsa = RSA.Create();

        string x = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        string y = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

        Console.WriteLine($"Public: {x}");
        Console.WriteLine($"Private: {y}");

        return Ok(new { Public = x, Private = y });
    }

    /// <summary>
    /// Method available to retrieve current Web API app information config
    /// </summary>
    /// <returns>DTO Key Value Response</returns>
    [HttpGet("Info", Name = nameof(GetInfo))]
    [ProducesResponseType(typeof(DTOList<DTOKeyValueResponse>), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<DTOList<DTOKeyValueResponse>> GetInfo()
    {
        TimeZoneInfo localZone = TimeZoneInfo.Local;
        DateTime currentDate = DateTime.Now;
        DateTime utcDate = DateTime.UtcNow;
        TimeSpan currentOffset = localZone.GetUtcOffset(currentDate);

        CultureInfo culture = CultureInfo.CurrentCulture;

        DTOList<DTOKeyValueResponse> dtoInfos =
        [
            new("Time Zone Name", localZone.StandardName),
            new("Time Zone Display Name", localZone.DisplayName),
            new("UTC Time Zone offset:", currentOffset.ToString()),
            new("Time Now", currentDate.ToString()),
            new("UTC Time Now", utcDate.ToString()),
            new("Culture Name", culture.TextInfo.CultureName),
            new("Currency Symbol (Money/Coin)", culture.NumberFormat.CurrencySymbol),
            new("Decimal Digits", culture.NumberFormat.CurrencyDecimalDigits.ToString()),
            new("Decimal Separator", culture.NumberFormat.CurrencyDecimalSeparator)
        ];

        return Ok(dtoInfos);
    }

    /// <summary>
    /// Method available get an Address by CEP
    /// </summary>
    /// <param name="aCep">CEP number</param>
    /// <returns>DTOFoundAddress</returns>
    /// <response code="200">Address found</response>  
    /// <response code="400">Invalid CEP</response>  
    /// <response code="404">Address not found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("cep/{aCep}", Name = nameof(GetCEPAsync))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DTOFoundAddress), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    public async Task<ActionResult<DTOFoundAddress>> GetCEPAsync([FromRoute] string aCep)
        => Ok(await new ViaCEP().GetAddressAsync(aCep));

    /// <summary>
    /// Method available get Brasil Regions list from Enum List.
    /// </summary>
    /// <returns>DTOEnum</returns>
    /// <response code="200">Address found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("Regions", Name = nameof(GetRegions))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(DTOEnum), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<DTOEnum> GetRegions()
        => Ok(GetFullEnumerator(GetRoots<Region>(), "Regiões"));

    /// <summary>
    /// Method available get states of Brasil Region from Enum List.
    /// </summary>
    /// <returns>DTOEnum</returns>
    /// <response code="200">Address found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("Regions/{aRegion}", Name = nameof(GetRegionUFs))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(DTOEnum), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<DTOEnum> GetRegionUFs([FromRoute] Region aRegion)
    {
        if (!aRegion.IsRoot())
            return BadRequest("Only root options available on route.");

        return Ok(GetFullEnumerator(aRegion.GetChildren(), $"Região {aRegion.GetDescription()}"));
    }

    /// <summary>
    /// Method available get UF list.
    /// It returns Region data with each state.
    /// </summary>
    /// <param name="aCep">CEP number</param>
    /// <returns>List of DTOState</returns>
    /// <response code="200">Address found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("UFs", Name = nameof(GetStatesAsync))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(IList<DTOState>), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    public async Task<ActionResult<IList<DTOState>>> GetStatesAsync()
        => Ok(await new IBGE().GetStatesAsync());

    /// <summary>
    /// Method available get UF list from Enum list.
    /// This method is faster than a bullet! Use this to fill combo.
    /// </summary>
    /// <returns>List f DTOEnumContent</returns>
    /// <response code="200">Address found</response>
    /// <response code="404">Address not found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("UFs/ForCombo", Name = nameof(GetStatesForCombo))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<IList<DTOEnumContent>> GetStatesForCombo()
        => Ok(GetEnumContent<UF>().OrderBy(uf => uf.Description));

    /// <summary>
    /// Method available get UF information from IBGE by Enum.
    /// It returns Region data for state.
    /// </summary>
    /// <param name="aUF">UF Enum</param>
    /// <returns>DTOState</returns>
    /// <response code="200">Address found</response>
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("UFs/{aUF}", Name = nameof(GetStateAsync))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DTOState), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    public async Task<ActionResult<DTOState>> GetStateAsync([FromRoute] UF aUF)
        => Ok(await new IBGE().GetStatesAsync(aUF));

    /// <summary>
    /// Method available get an Address by CEP
    /// </summary>
    /// <param name="aUF">UF Enum</param>
    /// <returns>List of DTOCity</returns>
    /// <response code="200">Address found</response>
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("UFs/{aUF}/Cities", Name = nameof(GetStatesCitiesAsync))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IList<DTOCity>), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    public async Task<ActionResult<IList<DTOCity>>> GetStatesCitiesAsync([FromRoute] UF aUF)
        => Ok(await new IBGE().GetStatesCitiesAsync(aUF));

    /// <summary>
    /// Method available get an Address by CEP
    /// </summary>
    /// <param name="aCityId">City IBGE Id</param>
    /// <returns>DTOCity</returns>
    /// <response code="200">Address found</response>
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("Cities/{aCityId}", Name = nameof(GetCitiesAsync))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DTOCity), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    public async Task<ActionResult<DTOCity>> GetCitiesAsync([FromRoute] int aCityId)
    {
        DTOCity dto = await new IBGE().GetCitiesAsync(aCityId);

        if (dto.IsNull())
            return NotFound();
        else
            return Ok(dto);
    }

    /// <summary>
    /// Method available to verify if CPF or CNPJ is valid
    /// </summary>
    /// <param name="aDocumentType">Document Type Enum Based</param>
    /// <param name="aDocumentNumber">DocumentNumber</param>
    /// <returns>Bool</returns>
    /// <response code="200">Valid or Not</response>
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("Document/Valid", Name = nameof(GetDocumentValid))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DTOBoolResponse), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<DTOBoolResponse> GetDocumentValid([FromQuery] DocumentType aDocumentType, [FromQuery] string aDocumentNumber)
    {
        if (!aDocumentType.Equals(DocumentType.CNPJ) && !aDocumentType.Equals(DocumentType.CPF))
            return BadRequest(string.Format("Only {0} or {1} allowed for {2}", DocumentType.CPF.GetDescription(), DocumentType.CNPJ.GetDescription(), nameof(aDocumentType)));

        if (aDocumentNumber.OnlyNumeric().IsNothing())
            return BadRequest(string.Format(ApplicationStatusMessage.InvalidParameter, nameof(aDocumentNumber)));

        return Ok(new DTOBoolResponse(Document.VerifyCPForCNPJ(aDocumentNumber.OnlyNumeric())));
    }

    /// <summary>
    /// Method available to get Company information by its Cnpj 
    /// </summary>
    /// <remarks>
    /// Use this method to retrieve Company information by its Cnpj
    /// 
    /// Business rule: **[LPAY-138]**
    /// 
    /// *Authentication required*
    /// </remarks>
    /// <param name="aCnpj">Cnpj</param>
    /// <returns>DTOBrasilApiCnpjResponse</returns>
    /// <response code="404">Not Found</response>
    /// <response code="200">Ok</response>
    [HttpGet("Cnpj/{aCnpj}", Name = nameof(GetCnpjInfoAsync))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DTOBrasilApiCnpjResponse), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    public async Task<ActionResult<DTOBrasilApiCnpjResponse>> GetCnpjInfoAsync([FromRoute] string aCnpj)
        => Ok(await new BrasilApi().GetCnpjInfoAsync(aCnpj));

    /// <summary>
    /// Method available get Day list from Enum List.
    /// </summary>
    /// <returns>DTOEnum</returns>
    /// <response code="200">Day found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("Days", Name = nameof(GetDays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(IList<DTOEnumContent>), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<IList<DTOEnumContent>> GetDays()
        => Ok(GetEnumContent<Day>());

    /// <summary>
    /// Method available get Month list from Enum List.
    /// </summary>
    /// <remarks>
    /// Inform date period to retrieve months between'em.
    /// </remarks>
    /// <returns>DTOEnum</returns>
    /// <response code="200">Month found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("Months", Name = nameof(GetMonths))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(IList<DTOEnumContent>), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<IList<DTOEnumContent>> GetMonths([FromQuery] DTOPeriodRequest aDtoRequest)
    {
        if (aDtoRequest.Since.HasValue && aDtoRequest.Until.HasValue)
            return Ok(GetFullEnumerator(aDtoRequest.Since.Value.GetMonthsBetween(aDtoRequest.Until.Value)));
        else if (aDtoRequest.Since.HasValue && !aDtoRequest.Until.HasValue)
            return Ok(GetFullEnumerator(aDtoRequest.Since.Value.GetMonthsBetween(new DateTime(DateTime.Now.Year, 12, 31))));
        else if (!aDtoRequest.Since.HasValue && aDtoRequest.Until.HasValue)
            return Ok(GetFullEnumerator(new DateTime(DateTime.Now.Year, 1, 1).GetMonthsBetween(aDtoRequest.Until.Value)));
        else
            return Ok(GetEnumContent<Month>());
    }

    /// <summary>
    /// Method available get Month from Month Days list from Enum List.
    /// </summary>
    /// <returns>DTOEnum</returns>
    /// <response code="200">Month found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("YearMonths", Name = nameof(GetYearMonths))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(DTOEnum), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<DTOEnum> GetYearMonths()
        => Ok(GetFullEnumerator(GetRoots<MonthDay>(), "Meses do Ano"));

    /// <summary>
    /// Method available get Month Days from Month from Enum List.
    /// </summary>
    /// <remarks>
    /// Inform Year to get days in month.
    /// </remarks>
    /// <param name="aMonthDay">Month day enum</param>
    /// <param name="aYear">Optional Year</param>
    /// <returns>DTOEnum</returns>
    /// <response code="200">Month days found</response>  
    /// <response code="500">Internal Server Error</response>  
    [HttpGet("MonthDays/{aMonthDay}", Name = nameof(GetMonthDays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(DTOEnum), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    [AllowAnonymous]
    public ActionResult<DTOEnum> GetMonthDays([FromRoute] MonthDay aMonthDay, [FromQuery] int? aYear)
    {
        if (!aMonthDay.IsRoot())
            return BadRequest("Only root options available on route.");

        DTOEnum monthDays = GetFullEnumerator(aMonthDay.GetChildren(), $"Dias do Mês de {aMonthDay.GetDescription()}");

        if (aYear.HasValue)
        {
            int days = DateTime.DaysInMonth(aMonthDay.GetValue() / 100, aYear.Value);

            monthDays.DtoEnumContents = monthDays.DtoEnumContents.Where(md => md.Id <= days);
        }

        return Ok(monthDays);
    }

    /// <summary>
    /// Method available to get Holidays Information by year
    /// </summary>
    /// <remarks>
    /// Use this method to retrieve information from all holidays from Brazil by year
    /// 
    /// Business rule: **[LPAY-138]**
    /// 
    /// *Authentication required*
    /// </remarks>
    /// <param name="aYear">Year</param>
    /// <returns>IList of DTOBrasilApiHolidayResponse</returns>
    /// <response code="404">Not Found</response>
    /// <response code="200">Ok</response>
    [HttpGet("Holidays/{aYear:int}", Name = nameof(GetHolidaysInfoAsync))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DTOBrasilApiHolidayResponse), StatusCodes.Status200OK)]
    [Produces(Constant.ApplicationProblemJson)]
    public async Task<ActionResult<DTOBrasilApiHolidayResponse>> GetHolidaysInfoAsync([FromRoute] int aYear)
        => Ok(await new BrasilApi().GetHolidaysInfoAsync(aYear));
}
