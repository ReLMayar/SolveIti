using Microsoft.AspNetCore.Mvc;
using SolveIti.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml.Linq;

namespace SolveIti.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        private readonly IConfiguration _configuration;
        SqlConnection connection;
        GeneralModel gm = new GeneralModel();

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            connection = new SqlConnection(_configuration.GetConnectionString("Union"));
            connection.Open();
            gm.stm = getSystemTypes();
        }

        public IActionResult Index()
        {
            return View(gm);
        }

        public IActionResult Documentations()
        {
            gm.documentationModels = getDocumentations();
            return View(gm);
        }
        private List<DocumentationModel> getDocumentations()
        {
            List<DocumentationModel> dm = new List<DocumentationModel>();

            dm.Add(new DocumentationModel() { Name = "АРМ \"Старший бухгалтер\"", DownloadPath = "https://dev.epd47.ru/Docs/Руководство АРМ Старший бухгалтер.pdf" });
            dm.Add(new DocumentationModel() { Name = "АРМ \"Бухгалтер\"", DownloadPath = "https://dev.epd47.ru/Docs/2014Руководство АРМ Бухгалтер.doc" });
            dm.Add(new DocumentationModel() { Name = "АРМ \"Кассир\"", DownloadPath = "https://dev.epd47.ru/Docs/Руководство АРМ Кассир.pdf" });
            dm.Add(new DocumentationModel() { Name = "АРМ \"Отчеты\"", DownloadPath = "https://dev.epd47.ru/Docs/Руководство АРМ Отчеты.pdf" });
            dm.Add(new DocumentationModel() { Name = "АРМ \"Печать извещений\"", DownloadPath = "https://dev.epd47.ru/Docs/Руководство АРМ Печать извещений.pdf" });

            return dm;
        }

        [HttpPost]
        public IActionResult SystemTypeView(string typeName)
        {
            gm.systemTypeSystemsModel = new SystemsSystemTypeModel();
            gm.systemTypeSystemsModel.Systems = getSystems(typeName);
            gm.systemTypeSystemsModel.systemTypeModel = gm.stm.First(q => q.Name == typeName);
            return View(gm);
        }

        private List<SystemTypeModel> getSystemTypes()
        {
            List<SystemTypeModel>  stm = new List<SystemTypeModel>();
            using (SqlCommand selectSystemTypes = new SqlCommand())
            {
                selectSystemTypes.Connection = connection;
                selectSystemTypes.CommandType = System.Data.CommandType.StoredProcedure;
                selectSystemTypes.CommandText = "si.GetSystemTypes";

                using (SqlDataReader reader = selectSystemTypes.ExecuteReader())
                {
                    while (reader.Read())
                        stm.Add(new SystemTypeModel()
                        {
                            Name = reader["name"].ToString(),
                            SvgHref = reader["svgHref"].ToString(),
                            AspAction = reader["aspAction"].ToString()
                        });
                }
            }
            return stm;
        }
        private List<SystemModel> getSystems(string typeName)
        {
            List<SystemModel> sm = new List<SystemModel>();
            using (SqlCommand selectSystems = new SqlCommand())
            {
                selectSystems.Connection = connection;
                selectSystems.CommandType = System.Data.CommandType.StoredProcedure;
                selectSystems.CommandText = "si.GetSystems";
                selectSystems.Parameters.AddWithValue("typeName", typeName);

                using (SqlDataReader reader = selectSystems.ExecuteReader())
                {
                    while (reader.Read())
                        sm.Add(new SystemModel()
                        {
                            id = Convert.ToInt64(reader["id"]),
                            idName = reader["id"].ToString(),
                            Name = reader["Name"].ToString(),
                            ApplicationsOfTheSystem = new List<ApplicationsOfTheSystemModel>()
                        });
                }
            }

            foreach (var item in sm)
                item.ApplicationsOfTheSystem = getApplicationsOfTheSystems(item.id);
            return sm;
        }
        private List<ApplicationsOfTheSystemModel> getApplicationsOfTheSystems(long systemId)
        {
            List<ApplicationsOfTheSystemModel> aots = new List<ApplicationsOfTheSystemModel>();
            using (SqlCommand selectApplicationsOfTheSystems = new SqlCommand())
            {
                selectApplicationsOfTheSystems.Connection = connection;
                selectApplicationsOfTheSystems.CommandType = System.Data.CommandType.StoredProcedure;
                selectApplicationsOfTheSystems.CommandText = "si.GetApplicationsOfTheSystems";
                selectApplicationsOfTheSystems.Parameters.AddWithValue("systemId", systemId);

                using (SqlDataReader reader = selectApplicationsOfTheSystems.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        aots.Add(new ApplicationsOfTheSystemModel()
                        {
                            Name = reader["Name"].ToString(),
                            DownloadPath = reader["DownloadPath"].ToString()
                        });
                    }
                }
            }
            return aots;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}