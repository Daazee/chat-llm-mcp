using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MsDynamics365MCP.MCPServer.Tools;

[McpServerToolType]
public sealed class CrmTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "get_contacts")]
    [Description("Retrieve contacts from Dynamics 365 CRM, optionally filtered by name or email.")]
    public static string GetContacts(
        [Description("Optional partial name to filter contacts by")] string? nameFilter = null,
        [Description("Optional email domain to filter contacts by")] string? emailDomain = null)
    {
        var contacts = new[]
        {
            new { Id = "C001", FirstName = "James", LastName = "Anderson", Email = "j.anderson@contoso.com", Phone = "+1-555-0101", Account = "Contoso Ltd", JobTitle = "IT Director", ModifiedOn = "2025-06-01" },
            new { Id = "C002", FirstName = "Sarah", LastName = "Mitchell", Email = "s.mitchell@fabrikam.com", Phone = "+1-555-0102", Account = "Fabrikam Inc", JobTitle = "Procurement Manager", ModifiedOn = "2025-06-10" },
            new { Id = "C003", FirstName = "David", LastName = "Chen", Email = "d.chen@northwind.com", Phone = "+1-555-0103", Account = "Northwind Traders", JobTitle = "CFO", ModifiedOn = "2025-05-28" },
            new { Id = "C004", FirstName = "Emma", LastName = "Wilson", Email = "e.wilson@contoso.com", Phone = "+1-555-0104", Account = "Contoso Ltd", JobTitle = "Operations Lead", ModifiedOn = "2025-06-15" },
            new { Id = "C005", FirstName = "Raj", LastName = "Patel", Email = "r.patel@tailspintoys.com", Phone = "+1-555-0105", Account = "Tailspin Toys", JobTitle = "CTO", ModifiedOn = "2025-06-20" }
        };

        var filtered = contacts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
            filtered = filtered.Where(c =>
                c.FirstName.Contains(nameFilter, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(emailDomain))
            filtered = filtered.Where(c =>
                c.Email.EndsWith(emailDomain, StringComparison.OrdinalIgnoreCase));

        var result = filtered.ToArray();
        return JsonSerializer.Serialize(new { total = result.Length, contacts = result }, JsonOptions);
    }

    [McpServerTool(Name = "get_accounts")]
    [Description("Retrieve accounts (companies/organisations) from Dynamics 365 CRM.")]
    public static string GetAccounts(
        [Description("Optional industry filter (e.g. 'Technology', 'Manufacturing')")] string? industry = null)
    {
        var accounts = new[]
        {
            new { Id = "A001", Name = "Contoso Ltd", Industry = "Technology", AnnualRevenue = 12_500_000m, Employees = 340, City = "Seattle", Country = "USA", AccountManager = "Sarah Mitchell", Status = "Active" },
            new { Id = "A002", Name = "Fabrikam Inc", Industry = "Manufacturing", AnnualRevenue = 8_700_000m, Employees = 210, City = "Chicago", Country = "USA", AccountManager = "David Chen", Status = "Active" },
            new { Id = "A003", Name = "Northwind Traders", Industry = "Retail", AnnualRevenue = 4_200_000m, Employees = 95, City = "London", Country = "UK", AccountManager = "Emma Wilson", Status = "Active" },
            new { Id = "A004", Name = "Tailspin Toys", Industry = "Consumer Goods", AnnualRevenue = 2_100_000m, Employees = 60, City = "Boston", Country = "USA", AccountManager = "James Anderson", Status = "Prospect" },
            new { Id = "A005", Name = "Adventure Works", Industry = "Technology", AnnualRevenue = 31_000_000m, Employees = 820, City = "San Francisco", Country = "USA", AccountManager = "Raj Patel", Status = "Active" }
        };

        var filtered = accounts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(industry))
            filtered = filtered.Where(a => a.Industry.Contains(industry, StringComparison.OrdinalIgnoreCase));

        var result = filtered.ToArray();
        return JsonSerializer.Serialize(new { total = result.Length, accounts = result }, JsonOptions);
    }

    [McpServerTool(Name = "get_opportunities")]
    [Description("Retrieve sales opportunities from Dynamics 365 CRM.")]
    public static string GetOpportunities(
        [Description("Filter by stage: 'Prospecting', 'Qualification', 'Proposal', 'Negotiation', 'Closed Won', 'Closed Lost'")] string? stage = null,
        [Description("Filter by account name")] string? accountName = null)
    {
        var opportunities = new[]
        {
            new { Id = "O001", Name = "Contoso ERP Upgrade", Account = "Contoso Ltd", Value = 250_000m, Stage = "Proposal", Probability = 60, CloseDate = "2025-08-31", Owner = "Sarah Mitchell" },
            new { Id = "O002", Name = "Fabrikam Cloud Migration", Account = "Fabrikam Inc", Value = 185_000m, Stage = "Negotiation", Probability = 80, CloseDate = "2025-07-15", Owner = "David Chen" },
            new { Id = "O003", Name = "Northwind POS System", Account = "Northwind Traders", Value = 72_000m, Stage = "Qualification", Probability = 40, CloseDate = "2025-09-30", Owner = "Emma Wilson" },
            new { Id = "O004", Name = "Adventure Works BI Platform", Account = "Adventure Works", Value = 640_000m, Stage = "Closed Won", Probability = 100, CloseDate = "2025-05-20", Owner = "Raj Patel" },
            new { Id = "O005", Name = "Tailspin Inventory Suite", Account = "Tailspin Toys", Value = 48_000m, Stage = "Prospecting", Probability = 20, CloseDate = "2025-10-15", Owner = "James Anderson" }
        };

        var filtered = opportunities.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(stage))
            filtered = filtered.Where(o => o.Stage.Equals(stage, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(accountName))
            filtered = filtered.Where(o => o.Account.Contains(accountName, StringComparison.OrdinalIgnoreCase));

        var result = filtered.ToArray();
        return JsonSerializer.Serialize(new { total = result.Length, opportunities = result }, JsonOptions);
    }

    [McpServerTool(Name = "create_lead")]
    [Description("Create a new sales lead in Dynamics 365 CRM.")]
    public static string CreateLead(
        [Description("First name of the lead")] string firstName,
        [Description("Last name of the lead")] string lastName,
        [Description("Email address of the lead")] string email,
        [Description("Company name")] string companyName,
        [Description("Phone number")] string? phone = null,
        [Description("Brief description of the lead's interest or topic")] string? description = null)
    {
        var leadId = $"L{Random.Shared.Next(1000, 9999)}";
        var lead = new
        {
            Id = leadId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Company = companyName,
            Phone = phone ?? "N/A",
            Description = description ?? string.Empty,
            Status = "New",
            CreatedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            Message = $"Lead '{firstName} {lastName}' from '{companyName}' successfully created with ID {leadId}."
        };

        return JsonSerializer.Serialize(lead, JsonOptions);
    }

    [McpServerTool(Name = "get_cases")]
    [Description("Retrieve customer support cases from Dynamics 365 CRM.")]
    public static string GetCases(
        [Description("Filter by status: 'Active', 'Resolved', 'Cancelled'")] string? status = null,
        [Description("Filter by priority: 'High', 'Normal', 'Low'")] string? priority = null)
    {
        var cases = new[]
        {
            new { Id = "CS001", Title = "Login failure after password reset", Account = "Contoso Ltd", Priority = "High", Status = "Active", CreatedOn = "2025-06-18", Owner = "Support Team A", Resolution = (string?)"" },
            new { Id = "CS002", Title = "Report export not generating PDF", Account = "Fabrikam Inc", Priority = "Normal", Status = "Active", CreatedOn = "2025-06-19", Owner = "Support Team B", Resolution = (string?)"" },
            new { Id = "CS003", Title = "Slow performance on dashboard", Account = "Adventure Works", Priority = "High", Status = "Resolved", CreatedOn = "2025-06-10", Owner = "Support Team A", Resolution = (string?)"Optimised database query and rebuilt index." },
            new { Id = "CS004", Title = "Missing email notifications", Account = "Northwind Traders", Priority = "Normal", Status = "Resolved", CreatedOn = "2025-06-12", Owner = "Support Team B", Resolution = (string?)"SMTP relay configuration corrected." },
            new { Id = "CS005", Title = "API rate limit errors", Account = "Tailspin Toys", Priority = "Low", Status = "Active", CreatedOn = "2025-06-21", Owner = "Support Team C", Resolution = (string?)"" }
        };

        var filtered = cases.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status))
            filtered = filtered.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(priority))
            filtered = filtered.Where(c => c.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));

        var result = filtered.ToArray();
        return JsonSerializer.Serialize(new { total = result.Length, cases = result }, JsonOptions);
    }

    [McpServerTool(Name = "get_pipeline_summary")]
    [Description("Get a high-level summary of the sales pipeline value and stage breakdown.")]
    public static string GetPipelineSummary()
    {
        var summary = new
        {
            TotalPipelineValue = 1_195_000m,
            Currency = "USD",
            Stages = new[]
            {
                new { Stage = "Prospecting", Count = 1, Value = 48_000m },
                new { Stage = "Qualification", Count = 1, Value = 72_000m },
                new { Stage = "Proposal", Count = 1, Value = 250_000m },
                new { Stage = "Negotiation", Count = 1, Value = 185_000m },
                new { Stage = "Closed Won", Count = 1, Value = 640_000m }
            },
            WinRate = "33%",
            AverageDealSize = 239_000m,
            AsOf = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };

        return JsonSerializer.Serialize(summary, JsonOptions);
    }
}
