using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using JenkinsDotNet;
using System.Text;
using System.IO;
using System.Xml.Linq;
using WebApplication1.Models;


namespace JenkinsAPI.Controllers
{
    //[Authorize]
    public class ValuesController : ApiController

    {
        // GET api/values
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}


        // GET api/values

        JenkinsDotNet.JenkinsServer _jenkinsServer = new JenkinsDotNet.JenkinsServer("http://10.233.4.210:8080", "g.madanagopal", "9b2d406a6c019fe2a4cdff12da0814b3");
        HttpAdapter _httpAdapter = new HttpAdapter();

        // GET api/values        
        public async Task<IHttpActionResult> GetJobDetails()
        {
            JenkinsDotNet.Model.Node _node = new JenkinsDotNet.Model.Node();
            _node = await _jenkinsServer.GetNodeDetailsAsync();
            return Ok(_node.Jobs);
        }

        // GET api/values/{projectToken}

        public IHttpActionResult GetTriggerBuild(string projectToken)
        {
            //_httpAdapter.Infrastructure.HttpAdapter _httpAdapter = new Jenkins_CSharp_Api.Infrastructure.HttpAdapter();
            string response = _httpAdapter.TriggerBuild(projectToken);
            return Ok(response);
        }

        // POST api/values
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT api/values/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE api/values/5
        public void Delete(int id)
        {
        }



    }
}

