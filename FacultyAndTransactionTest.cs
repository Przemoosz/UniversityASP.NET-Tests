using System;
using System.Collections.Generic;
using System.Linq;
using FirstProject.Controllers;
using FirstProject.Data;
using FirstProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using FirstProject.Models.Enums;
using FirstProject.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Debugger.Contracts.HotReload;

namespace UniversityManagementTest;

[TestClass]
public class FacultyCreateTest
{
    // Testing Faculty/Create
    // Faculty can not be created without university
    // This require class init 

    // Database context options
    private static DbContextOptions<ApplicationDbContext>? _options;

    // Default University
    private static University _university = new University()
    {
        UniversityName = "Test1",
        Adress = "TestAdress1",
        CreationDate = DateTime.Now,
        Employed = 60,
        Faculties = new List<Faculty>(2)
    };
    
    [ClassInitialize]
    public static async Task ClassInit(TestContext testContext)
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FacultyDatabaseTest").Options;
       await using (var context = new ApplicationDbContext(_options))
        {
            await context.AddAsync(_university);
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Faculty_Successful_Create_Test()
    {
        // Controller should save Faculty in database
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            FacultyController facultyController = new FacultyController(context);
            Faculty testFaculty = new Faculty()
            {
                FacultyID = 1,
                FacultyName = "TestFaculty",
                Employed = 30,
                Budget = 1000m,
                CreationDate = DateTime.Now,
                UniversityID = 1
            };
            
            // Act Section
            var a = await facultyController.Create(testFaculty);
            // await context.Faculty.AddAsync(testFaculty);
            // await context.SaveChangesAsync();
            
            // Assert Section
            var facultyQuery = from faculty in context.Faculty select faculty;
            Assert.AreEqual(1, await facultyQuery.CountAsync());
            var fetchedFaculty = await context.Faculty.FirstOrDefaultAsync(f => f.FacultyID == 1);
            Assert.AreNotEqual(null, fetchedFaculty);
            Assert.AreEqual(1, fetchedFaculty.FacultyID);
            Assert.AreEqual(1, fetchedFaculty.UniversityID);
            Assert.AreEqual(30, fetchedFaculty.Employed);
            Assert.AreEqual("TestFaculty", fetchedFaculty.FacultyName);
        }
    }

    [TestMethod]
    public async Task Faculty_Null_University_Create_Test()
    {
        // Controller should return NotFound ViewResult if choosed university does not exists in database
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            var controller = new FacultyController(context);
            Faculty testFaculty = new Faculty()
            {
                FacultyID = 1,
                FacultyName = "TestFaculty",
                Employed = 30,
                Budget = 1000m,
                CreationDate = DateTime.Now,
                UniversityID = 4500
            };
            
            // Act Section
            var result  = await controller.Create(testFaculty) as NotFoundResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            Assert.AreEqual(true, result is NotFoundResult);
        }
    }
    [TestCleanup]
    public async Task TestsCleanUp()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            var faculty = await context.Faculty.ToListAsync();
            if (faculty.Count != 0)
            {
                context.RemoveRange(faculty);
                await context.SaveChangesAsync();
            }
        }
    }

    [ClassCleanup]
    public static async Task ClassCleanUp()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            context.Remove(_university);
            await context.SaveChangesAsync();
        }
    }
}

[TestClass]
public class FacultyIndexTest
{
    // Testing data returned from UniversityController which is based on Faculty model

    // Database context options
    private static DbContextOptions<ApplicationDbContext>? _options;

    [ClassInitialize]
    public static async Task ClassTestInit(TestContext testContext)
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FacultyTestDb").Options;
        await using (var context = new ApplicationDbContext(_options))
        {
            Faculty faculty1 = new Faculty()
            {
                FacultyID = 1,
                FacultyName = "TestFaculty",
                Employed = 30,
                Budget = 1000m,
                CreationDate = DateTime.Now,
                UniversityID = 4500
            };
            Faculty faculty2 = new Faculty()
            {
                FacultyID = 2,
                FacultyName = "TestFaculty2",
                Employed = 30,
                Budget = 1000m,
                CreationDate = DateTime.Now,
                UniversityID = 4500
            };
            await context.Faculty.AddAsync(faculty1);
            await context.Faculty.AddAsync(faculty2);
            await context.University.AddAsync(new University()
            {
                UniversityName = "Test1",
                Adress = "TestAdress1",
                CreationDate = DateTime.Now,
                Employed = 60,
                Faculties = new List<Faculty>(2){faculty1,faculty2}
            });
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Faculty_Index_Test()
    {
        // Controller should return selected university with faculties attached to them
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            var controller = new UniversityController(context);
            
            // Act Section
            var viewResult = await controller.Index("Test1") as ViewResult;
            
            
            // Assert Section
            Assert.AreNotEqual(null,viewResult);
            var resultModel = (University) viewResult.Model;
            Assert.AreEqual(2, resultModel.Faculties.Count);
            var faculty = await context.Faculty.FirstOrDefaultAsync(f => f.FacultyID == 1);
            Assert.AreEqual(true, resultModel.Faculties.Contains(faculty));
        }
    }
    
    [TestMethod]
    public async Task Faculty_Index_NotFound_Test()
    {
        // Controller should return NotFoundResult if university name does not exists in database
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            var controller = new UniversityController(context);
            
            // Act Section
            var viewResult = await controller.Index("NotExistingUniversityName") as NotFoundResult;

            // Assert Section
            Assert.AreNotEqual(null,viewResult);
            Assert.AreEqual(true, viewResult is NotFoundResult);
        }
    }
    
    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            var unis = await context.University.ToListAsync();
            var faculties = await context.Faculty.ToListAsync();
            if (unis.Count != 0)
            {
                context.University.RemoveRange(unis);
            }
            if (faculties.Count != 0)
            {
                context.Faculty.RemoveRange(faculties);
            }
            await context.SaveChangesAsync();
        }
    }
}


[TestClass]
public class TransactionCreateTest
{
    // Testing Transaction Create class which is closely connected (Adding transaction changes budget in Faculty) to Faculty
    
    // Database Options
    private static DbContextOptions<ApplicationDbContext>? _options;

    private static Faculty _faculty1 = new Faculty()
    {
        FacultyID = 1,
        FacultyName = "TestFaculty",
        Employed = 30,
        Budget = 1000m,
        CreationDate = DateTime.Now,
        UniversityID = 4500
    };
    [ClassInitialize]
    public static async Task ClassTestInit(TestContext testContext)
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TransactionTestDb").Options;
    }

    [TestInitialize]
    public async Task TestInit()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            
            await context.Faculty.AddAsync(_faculty1);
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Transaction_Create_Test()
    {
        // Controller should create and save transaction in database
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            var controller = new TransactionController(context);
            Transaction testTransaction = new Transaction()
            {
                Amount = 100, 
                FacultyID = 1, 
                Group = TransactionGroupEnum.Buildings,
                TransactionDate = DateTime.Now,
                TransactionName = "TestName"
            };
            // Act section
            var result = await controller.Create(testTransaction);
            var resultTransaction = await context.Transaction.FirstOrDefaultAsync(t => t.TransactionID == 1);
            // Assert Section
            Assert.AreEqual(testTransaction.TransactionName, resultTransaction.TransactionName);

        }
    }

    [TestMethod]
    public async Task Transaction_Create_Get_ID_Test()
         {
             // Controller should return view with ViewData - FacultyName
             
             // Arrange Section
             await using (var context = new ApplicationDbContext(_options!))
             {
                 // Act Section
                 var controller = new TransactionController(context);
                 var result = await controller.Create(1) as ViewResult;
                 
                 // Assert Section
                 Assert.AreEqual("TestFaculty", result.ViewData["FacultyName"]);
             }
         }

    [TestMethod]
    public async Task Transaction_Create_Faculty_Budget_Outcome_Test()
    {
        // Controller should subtract Transaction.Amount from attached Faculty.Budget  
             
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            // Act Section
            var controller = new TransactionController(context);
            Transaction testTransaction = new Transaction()
            {
                Amount = 100, 
                FacultyID = 1, 
                Group = TransactionGroupEnum.Buildings,
                TransactionDate = DateTime.Now,
                TransactionName = "TestName"
            };
            
            await controller.Create(testTransaction);
            // Assert Section
            var baseFaculty = await context.Faculty.FirstAsync(f => f.FacultyID == 1);
            Assert.AreEqual(900, baseFaculty.Budget);
        }
    }
    [TestMethod]
    public async Task Transaction_Create_Faculty_Budget_Outcome_DifferentType_Test()
    {
        // Controller should subtract Transaction.Amount from attached Faculty.Budget - Testing for 4 types of outcome
             
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            // Act Section
            var controller = new TransactionController(context);
            Transaction testTransaction1 = new Transaction()
            {
                Amount = 100, 
                FacultyID = 1, 
                Group = TransactionGroupEnum.Buildings,
                TransactionDate = DateTime.Now,
                TransactionName = "TestName1"
            };
            Transaction testTransaction2 = new Transaction()
            {
                Amount = 100, 
                FacultyID = 1, 
                Group = TransactionGroupEnum.Electricity,
                TransactionDate = DateTime.Now,
                TransactionName = "TestName2"
            };
            Transaction testTransaction3 = new Transaction()
            {
                Amount = 100, 
                FacultyID = 1, 
                Group = TransactionGroupEnum.Other,
                TransactionDate = DateTime.Now,
                TransactionName = "TestName3"
            };
            Transaction testTransaction4 = new Transaction()
            {
                Amount = 100, 
                FacultyID = 1, 
                Group = TransactionGroupEnum.Service,
                TransactionDate = DateTime.Now,
                TransactionName = "TestName4"
            };
            await controller.Create(testTransaction1);
            await controller.Create(testTransaction2);
            await controller.Create(testTransaction3);
            await controller.Create(testTransaction4);

            // Assert Section
            var baseFaculty = await context.Faculty.FirstAsync(f => f.FacultyID == 1);
            Assert.AreEqual(600, baseFaculty.Budget);
        }
    }
    [TestMethod]
    public async Task Transaction_Create_Faculty_Budget_Income_Test()
    {
        // Controller should add Transaction. Amount to attached faculty.budget if transaction type is Income 
             
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            // Act Section
            var controller = new TransactionController(context);
            Transaction testTransaction = new Transaction()
            {
                Amount = 100, 
                FacultyID = 1, 
                Group = TransactionGroupEnum.Income,
                TransactionDate = DateTime.Now,
                TransactionName = "TestName"
            };
            
            await controller.Create(testTransaction);
            // Assert Section
            var baseFaculty = await context.Faculty.FirstAsync(f => f.FacultyID == 1);
            Assert.AreEqual(1100, baseFaculty.Budget);
        }
    }
    [TestMethod]
    public async Task Transaction_Create_Get_NullID_Test()
    {
        // Controller should return view with ViewData - FacultyName (null) and Faculty select list, to choose faculty on page
             
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            // Act Section
            var controller = new TransactionController(context);
            var result = await controller.Create(facultyID:null) as ViewResult;
            var returnedSelectList = result.ViewData["Faculty"] as SelectList;
            // Assert Section
            Assert.AreNotEqual(null, returnedSelectList);
            Assert.AreEqual(1, returnedSelectList.Count());
            Assert.AreEqual(null, result.ViewData["FacultyName"]);
        }
    }
    
    [TestCleanup]
    public async Task TestCleanup()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            context.Remove(_faculty1);
            await context.SaveChangesAsync();
        }
    }
    

    [ClassCleanup]
    public static async Task ClassCleanUp()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            var transaction = await context.Transaction.ToListAsync();
            var faculties = await context.Faculty.ToListAsync();
            if (transaction.Count != 0)
            {
                context.Transaction.RemoveRange(transaction);
            }
            if (faculties.Count != 0)
            {
                context.Faculty.RemoveRange(faculties);
            }
            await context.SaveChangesAsync();
        }
    }
}

[TestClass]
public class TransactionOtherTest
{
    // Testing Transaction Create class which is closely connected (Adding transaction changes budget in Faculty) to Faculty
    
    // Database Options
    private static DbContextOptions<ApplicationDbContext>? _options;

    private static Faculty _faculty1 = new Faculty()
    {
        FacultyID = 1,
        FacultyName = "TestFaculty",
        Employed = 30,
        Budget = 1000m,
        CreationDate = DateTime.Now,
        UniversityID = 4500
    };
    [ClassInitialize]
    public static async Task ClassTestInit(TestContext testContext)
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TransactionTestDb").Options;
    }

    [TestInitialize]
    public async Task TestInit()
    {
        await using (var context = new ApplicationDbContext(_options!))
        {
            
            await context.Faculty.AddAsync(_faculty1);
            await context.SaveChangesAsync();
        }
    }

    [TestCleanup]
    public async Task CleanUp()
    {
        await using (var context = new ApplicationDbContext(_options!))
        {
            context.Faculty.Remove(_faculty1);
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Transaction_Delete_Outcome_Test()
    {
        // Testing removing transaction from Faculty,
        // controller should update budget to starting one in faculty after removing attached transactions
        
        // Arrange Section
        Transaction testTransaction1 = new Transaction()
        {
            Amount = 100, 
            FacultyID = 1, 
            Group = TransactionGroupEnum.Buildings,
            TransactionDate = DateTime.Now,
            TransactionName = "TestName1"
        };
        Transaction testTransaction2 = new Transaction()
        {
            Amount = 100, 
            FacultyID = 1, 
            Group = TransactionGroupEnum.Electricity,
            TransactionDate = DateTime.Now,
            TransactionName = "TestName2"
        };
        Transaction testTransaction3 = new Transaction()
        {
            Amount = 100, 
            FacultyID = 1, 
            Group = TransactionGroupEnum.Other,
            TransactionDate = DateTime.Now,
            TransactionName = "TestName3"
        };
        Transaction testTransaction4 = new Transaction()
        {
            Amount = 100, 
            FacultyID = 1, 
            Group = TransactionGroupEnum.Service,
            TransactionDate = DateTime.Now,
            TransactionName = "TestName4"
        };
        
        // Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new TransactionController(context);
            await controller.Create(testTransaction1);
            await controller.Create(testTransaction2);
            await controller.Create(testTransaction3);
            await controller.Create(testTransaction4);
            
            // Testing new budget in faculty, should equal 600
            var testingFaculty = await context.Faculty.FirstAsync(f => f.FacultyID == 1);
            Assert.AreEqual(600,testingFaculty.Budget);
            
            // Removing transaction from faculty
            await controller.DeleteConfirmed(1);
            await controller.DeleteConfirmed(2);
            await controller.DeleteConfirmed(3);
            await controller.DeleteConfirmed(4);

            // Assert section
            Assert.AreEqual(1000,testingFaculty.Budget);
        }
        
    }
    [TestMethod]
    public async Task Transaction_Index_Objects_Test()
    {
        // Testing index view in transaction, controller should return a view with IEnumerable<Transaction>
        
        // Arrange Section
        Transaction testTransaction1 = new Transaction()
        {
            Amount = 100, 
            FacultyID = 1, 
            Group = TransactionGroupEnum.Buildings,
            TransactionDate = DateTime.Now,
            TransactionName = "TestName1"
        };
        Transaction testTransaction2 = new Transaction()
        {
            Amount = 100, 
            FacultyID = 1, 
            Group = TransactionGroupEnum.Electricity,
            TransactionDate = DateTime.Now,
            TransactionName = "TestName2"
        };
        
        // Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new TransactionController(context);
            await controller.Create(testTransaction1);
            await controller.Create(testTransaction2);

            var result = await controller.Index() as ViewResult;

            Assert.AreNotEqual(null, result);
            var returnedObjects = result!.Model as List<Transaction>;
            
            // Assert Section
            Assert.AreNotEqual(null, returnedObjects);
            Assert.AreEqual(returnedObjects![0].TransactionName, "TestName1");
            Assert.AreEqual(returnedObjects[1].TransactionName, "TestName2");
        }
        
    }
}
