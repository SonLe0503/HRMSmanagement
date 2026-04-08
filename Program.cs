using HRManagement.Configuration;
using HRManagement.DataAcess;
using HRManagement.DataAcess.Implementations;
using HRManagement.DataAcess.Interfaces;
using HRManagement.Filters;
using HRManagement.Mappers;
using HRManagement.Models;
using HRManagement.Services;
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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HrmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"))
);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IEmployeeDocumentRepository, EmployeeDocumentRepository>();
builder.Services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IHRProcedureRepository, HRProcedureRepository>();
builder.Services.AddScoped<IHRProcedureService, HRProcedureService>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IFaceVerificationService, FaceVerificationService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IShiftAssignmentService, ShiftAssignmentService>();
builder.Services.AddScoped<IShiftAssignmentRepository, ShiftAssignmentRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IFaceVerificationService, FaceVerificationService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<IEvaluationTemplateRepository, EvaluationTemplateRepository>();
builder.Services.AddScoped<IEvaluationTemplateService, EvaluationTemplateService>();
builder.Services.AddScoped<IEvaluationCycleRepository, EvaluationCycleRepository>();
builder.Services.AddScoped<IEvaluationCycleService, EvaluationCycleService>();
builder.Services.AddScoped<IEvaluationCriteriaRepository, EvaluationCriteriaRepository>();
builder.Services.AddScoped<IEvaluationCriteriaService, EvaluationCriteriaService>();
builder.Services.AddScoped<FaceEmbeddingService>();
builder.Services.AddScoped<IUserAccountValidationService, UserAccountValidationService>();
builder.Services.AddScoped<ITopLevelResolver, TopLevelResolver>();
builder.Services.AddScoped<IApprovalRouteService, ApprovalRouteService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IOvertimeRequestService, OvertimeRequestService>();
builder.Services.AddAutoMapper(typeof(TaskProfile).Assembly);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HR Management  API",
        Version = "v1",
        Description = "API Authentication with JWT for HR Management "
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });
// Configure file upload limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configure Kestrel server limits (optional)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 5 * 1024 * 1024; 
});
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();


