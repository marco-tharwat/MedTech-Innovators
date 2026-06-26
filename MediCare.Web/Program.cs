using MediCare.Data.Models;
using MediCare.Data.Repositories.Implementations;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Interfaces;
using MediCare.Services.Services.Implementation;
using MediCare.Web.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<MedContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();

//Services
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<MedContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// COPILOT: START temporary test seeding - remove before commit
// Seed Roles and sample users/profiles for testing
/////////////
using (var scope = app.Services.CreateScope())
{
    //////////////////////
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<MedContext>();

    // ///////////  Seed roles (Admin, Doctor, Patient)
    await AccountController.SeedRoles(roleManager);

    //////////// Ensure a specialization exists for the sample doctor
    if (!await context.Specializations.AnyAsync())
    {
        context.Specializations.Add(new Specialization { Name = "General" });
        await context.SaveChangesAsync();
    }

    ///////////// Create a sample doctor user + domain profile if not exists
    if (await userManager.FindByNameAsync("drsmith") == null)
    {
        var doctorUser = new ApplicationUser
        {
            UserName = "drsmith",
            Email = "dr.smith@example.com",
            FullName = "Dr John Smith",
            Gender = Gender.Male
        };
        await userManager.CreateAsync(doctorUser, "Password123!");
        await userManager.AddToRoleAsync(doctorUser, "Doctor");

        // create doctor profile and mark approved so UI is usable in tests
        var spec = await context.Specializations.FirstAsync();
        var doctor = new Doctor
        {
            UserId = doctorUser.Id,
            ConsultationFee = 50m,
            IsApproved = true,
            SpecializationId = spec.Id
        };
        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();
    }

    // Create a sample patient user + domain profile if not exists
    if (await userManager.FindByNameAsync("patient1") == null)
    {
        var patientUser = new ApplicationUser
        {
            UserName = "patient1",
            Email = "patient1@example.com",
            FullName = "Jane Patient",
            Gender = Gender.Female
        };
        await userManager.CreateAsync(patientUser, "Password123!");
        await userManager.AddToRoleAsync(patientUser, "Patient");

        var patient = new Patient
        {
            UserId = patientUser.Id,
            BirthDate = DateTime.Today.AddYears(-30)
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
    }
}
// COPILOT: END temporary test seeding - remove before commit

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
