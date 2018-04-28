﻿using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Zametek.Utility;

namespace Test.RestApi
{
    [Route("api/[controller]")]
    public class ValuesController
        : Controller
    {
        private readonly IValueAccess m_ValueAccess;
        private readonly ILogger m_Logger;

        public ValuesController(
            IValueAccess valueAccess,
            ILogger logger)
        {
            m_ValueAccess = valueAccess ?? throw new ArgumentNullException(nameof(valueAccess));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Debug.Assert(TrackingContext.Current != null);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RequestDto requestDto)
        {
            m_Logger.Information($"{nameof(Post)} Invoked");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string result = await m_ValueAccess.AddAsync(requestDto).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Error caught in the controller class.");
            }
            return BadRequest(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            m_Logger.Information($"{nameof(Get)} Invoked");
            try
            {
                IList<ResponseDto> responses = await m_ValueAccess.GetAsync().ConfigureAwait(false);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Error caught in the controller class.");
            }
            return BadRequest(HttpStatusCode.BadRequest);
        }
    }
}
