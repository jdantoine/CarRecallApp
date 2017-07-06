using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class CarsCController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Provides an object to collect parameters of user selection on front end. For use in populating drop-down menus and the results grid.
        /// </summary>
        public class Selected
        {
            public string year { get; set; }
            public string make { get; set; }
            public string model { get; set; }
            public string trim { get; set; }
        }

        /// <summary>
        /// Provides an object to capture the Car Id for use in the GetCarDetails function, returning recall information and an image.
        /// </summary>
        public class IdParam
        {
            public int id { get; set; }
        }

        /// <summary>
        /// List of unique years in table Cars for use in the dropdown menu, sorted by make
        /// </summary>
        /// <returns>All unique years in the column model_year</returns>
        [HttpPost]
        public IHttpActionResult GetUniqueYearsByMakeModel(Selected selected)
        {
            var _make = new SqlParameter("@make", selected.make ?? "");
            var _model_name = new SqlParameter("@model_name", selected.model ?? "");

            var returnValue = db.Database.SqlQuery<string>(
                "EXEC GetUniqueYearsByMakeModel @make, @model_name", _make, _model_name).ToList();
            return Ok(returnValue);
        }

        /// <summary>
        /// List of unique makes from table Cars for use in dropdown menu
        /// </summary>
        /// <param name="model_year">For defining contents of list for Cars:make</param>
        /// <returns>All unique makes for user-selected year (column make, column model_year)</returns>
        [HttpPost]
        public IHttpActionResult GetUniqueMakes()
        {
            var returnValue = db.Database.SqlQuery<string>("EXEC GetUniqueMakes").ToList();
            //with no parameters, take out @body_style through the id).;
            // mutiple parameters: create parameter ahead of the EXEC functions, then simply list @id, @body_style, var, var
            return Ok(returnValue);
        }

        /// <summary>
        /// All unique models from table Cars for use in the dropdown menu, sorted by year and make
        /// </summary>
        /// <param name="model_year">for defining list of Cars:make</param>
        /// <param name="make">for further defining list of Cars:model_name</param>
        /// <returns>All unique models for selected year and make</returns>
        [HttpPost]
        public IHttpActionResult GetUniqueModelsByMake(Selected selected)
        {
            var _make = new SqlParameter("@make", selected.make ?? "");

            var returnValue = db.Database.SqlQuery<string>(
                "EXEC GetUniqueModelsByMake @make", _make).ToList();

            return Ok(returnValue);
        }

        /// <summary>
        /// All unique trims from table Cars for use in the dropdown menu, sorted by make, model, and year
        /// </summary>
        /// <param name="model_year">for further defining list of Cars:make</param>
        /// <param name="make">for further defining list of Cars:make, model</param>
        /// <param name="model_name">for furher defining list of Cars:make, model, trim</param>
        /// <returns>all unique trims for user-selected year, make, model</returns>
        [HttpPost]
        public IHttpActionResult GetUniqueTrimsByMakeModelYear(Selected selected)
        {
            var _model_year = new SqlParameter("@model_year", selected.year ?? "");
            var _make = new SqlParameter("@make", selected.make ?? "");
            var _model_name = new SqlParameter("@model_name", selected.model ?? "");



            var returnValue = db.Database.SqlQuery<string>(
                "EXEC GetUniqueTrimsByMakeModelYear @model_year, @make, @model_name", _model_year, _make, _model_name).ToList();

            return Ok(returnValue);
        }

        /// <summary>
        /// Displays all cars from table Cars following the given user-selected parameters, including all columns provided in the database.
        /// </summary>
        /// <param name="model_year">for refining list of Cars:make</param>
        /// <param name="make">for refining list of Cars:make, model</param>
        /// <param name="model_name">for refining list of Cars:make, model, trim</param>
        /// <param name="model_trim">for refining list of Cars:make, model, trim, etc</param>
        /// <returns>Returns all cars fitting the user-selected parameters</returns>
        [HttpPost]
        public IHttpActionResult GetCars(Selected selected)
        {
            var _model_year = new SqlParameter("@model_year", selected.year ?? "");
            var _make = new SqlParameter("@make", selected.make ?? "");
            var _model_name = new SqlParameter("@model_name", selected.model ?? "");
            var _model_trim = new SqlParameter("@model_trim", selected.trim ?? "");

            var returnValue = db.Database.SqlQuery<string>(
                "EXEC GetCars @model_year, @make, @model_name, @model_trim", _model_year, _make, _model_name, _model_trim).ToList();

            return Ok(returnValue);
        }


        /// <summary>
        /// Uses the Id from object Car as selected by the user. Accesses recall information from the National Highway and Traffic Safety Administration API. Acquires an image through the Bing search API.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>




        [Route("carRecall")]
        public async Task<CarViewModel> getCarRecall(string year, string make, string model, string trim)
        {
            HttpResponseMessage response;
            var content = "";
            var car = new CarViewModel();
            

            //get car recall info from NHTSA
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://one.nhtsa.gov/");
                try
                {
                    response = await client.GetAsync("webapi/api/Recalls/vehicle/modelyear/" + year + "/make/" + make + "/model/" + model + "?format=json");
                    content = await response.Content.ReadAsStringAsync();
                    car.Recalls = JsonConvert.DeserializeObject<carRecall>(content);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    await Task.FromResult(e);
                }
                
            }
            //Bing Cognitive Img Search
            var query = year + " " + make + " " + model + " " + trim;
            var url = $"https://api.cognitive.microsoft.com/bing/v5.0/images/" + $"search?q={WebUtility.UrlEncode(query)}" + $"&count=20&offset=0&mkt=en-us&safeSearch=Strict";
            var requestHeaderKey = "Ocp-Apim-Subscription-Key";
            var requestHeaderValue = ConfigurationManager.AppSettings["BingSearchKey"];
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add(requestHeaderKey, requestHeaderValue);
                    var json = await client.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<SearchResult>(json);
                    car.ImgSearchResult = result.Images.Select(i => new ImageResults
                    {
                        ContextLink = i.HostPageUrl,
                        FileFormat = i.EncodingFormat,
                        ImageLink = i.ContentUrl,
                        ThumbnailLink = i.ThumbnailUrl,
                        Title = i.Name
                    }
                    );


                        
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                return null;
            }
            return car;
        }
        
    }
}
