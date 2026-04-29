using HRManagement.Configuration;
using HRManagement.DataAcess;
using HRManagement.DataAcess.Implementations;
using HRManagement.DataAcess.Interfaces;
using HRManagement.Filters;
using HRManagement.Mappers;
using HRManagement.Models;
using HRManagement.Services.Attendances;
using HRManagement.Services.Cloudinaries;
using HRManagement.Services.CurrentUsers;
using HRManagement.Services.Departments;
using HRManagement.Services.Emails;
using HRManagement.Services.Employees;
using HRManagement.Services.FaceVerifications;
using HRManagement.Services.FileStorages;
using HRManagement.Services.HRProceduces;
using HRManagement.Services.Leaves;
using HRManagement.Services.Overtimes;
using HRManagement.Services.Positions;
using HRManagement.Services.Shifts;
using HRManagement.Services.Users;
using HRManagement.Services.Approvals;
using HRManagement.Services.Evaluations;
using HRManagement.Services.Analytics;
using HRManagement.Services.Audits;
using HRManagement.Services.Exports;
using HRManagement.Services.Backgrounds;
using HRManagement.Services.Payroll;
using HRManagement.Services.Resignations;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<HrmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"))
);

// Infrastructure
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddAutoMapper(typeof(TaskProfile).Assembly);

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeDocumentRepository, EmployeeDocumentRepository>();
builder.Services.AddScoped<IHRProcedureRepository, HRProcedureRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<IShiftAssignmentRepository, ShiftAssignmentRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<IEvaluationTemplateRepository, EvaluationTemplateRepository>();
builder.Services.AddScoped<IEvaluationCycleRepository, EvaluationCycleRepository>();
builder.Services.AddScoped<IEvaluationCriteriaRepository, EvaluationCriteriaRepository>();
builder.Services.AddScoped<IEvaluationRatingRepository, EvaluationRatingRepository>();

// Payroll Repositories
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IPayrollPeriodRepository, PayrollPeriodRepository>();

// Core Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IHRProcedureService, HRProcedureService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IFaceVerificationService, FaceVerificationService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IShiftAssignmentService, ShiftAssignmentService>();
builder.Services.AddScoped<IOvertimeRequestService, OvertimeRequestService>();
builder.Services.AddScoped<IUserAccountValidationService, UserAccountValidationService>();
builder.Services.AddScoped<ITopLevelResolver, TopLevelResolver>();
builder.Services.AddScoped<IApprovalRouteService, ApprovalRouteService>();
builder.Services.AddScoped<FaceEmbeddingService>();

// Specialized Services (Evaluation, Analytics, etc.)
builder.Services.AddScoped<IEvaluationService, EvaluationService>();
builder.Services.AddScoped<IEvaluationTemplateService, EvaluationTemplateService>();
builder.Services.AddScoped<IEvaluationCycleService, EvaluationCycleService>();
builder.Services.AddScoped<IEvaluationCriteriaService, EvaluationCriteriaService>();
builder.Services.AddScoped<ISubmitEvaluationService, SubmitEvaluationService>();
builder.Services.AddScoped<IViewEvaluationResultService, ViewEvaluationResultService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IWorkforceAnalyticsService, WorkforceAnalyticsService>();
builder.Services.AddScoped<ICompetencyReportService, CompetencyReportService>();
builder.Services.AddScoped<IExportService, ExportService>();

// Resignation Request
builder.Services.AddScoped<IResignationRequestService, ResignationRequestService>();

// Payroll Services
builder.Services.AddScoped<TaxCalculationService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

builder.Services.AddHostedService<HRProcedureBackgroundService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:5173", "https://app.peoplecore.tech")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              );
});

// Swagger/API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HR Management API",
        Version = "v1",
        Description = "API Authentication with JWT for HR Management"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token ở dạng: Bearer {token}"
    });

    c.OperationFilter<AuthorizeCheckOperationFilter>();
});

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;
                var userIdStr = principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var tokenLastLogin = principal?.FindFirst("LastLogin")?.Value;

                if (string.IsNullOrEmpty(tokenLastLogin))
                {
                    context.Fail("Invalid or outdated token format. Please re-login.");
                    return;
                }

                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                {
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<HrmsDbContext>();
                    var user = await dbContext.Users.FindAsync(userId);
                    
                    if (user != null && user.LastLogin.HasValue)
                    {
                        var dbLastLogin = user.LastLogin.Value.ToString("yyyyMMddHHmmss");
                        if (dbLastLogin != tokenLastLogin)
                        {
                            context.Fail("Concurrent login detected. Session is no longer valid.");
                        }
                    }
                }
            }
        };
    });

builder.Services.AddAuthorization();

// Controllers & JSON configuration
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// File upload limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 5 * 1024 * 1024;
});

var app = builder.Build();
app.UseRouting();

// Middleware Pipeline
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
