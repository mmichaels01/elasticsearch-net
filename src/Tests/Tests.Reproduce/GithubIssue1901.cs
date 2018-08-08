﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using Nest;
using FluentAssertions;
using Tests.Core;
using Tests.Core.Client;

namespace Tests.Reproduce
{
	public class GithubIssue1901
	{
		private class Example
		{
		}

		private const string ProxyAuthResponse = @"<html>
<head><title>401 Authorization Required</title></head>
<body bgcolor=""white"">
<center><h1>401 Authorization Required</h1></center>
<hr><center>nginx/1.4.6 (Ubuntu)</center>
</body>
</html>
<!-- a padding to disable MSIE and Chrome friendly error page -->
<!-- a padding to disable MSIE and Chrome friendly error page -->
<!-- a padding to disable MSIE and Chrome friendly error page -->
<!-- a padding to disable MSIE and Chrome friendly error page -->
<!-- a padding to disable MSIE and Chrome friendly error page -->
<!-- a padding to disable MSIE and Chrome friendly error page -->";

		[U] public async Task BadAuthResponseDoesNotThrowExceptionWhenAttemptingToDeserializeResponse()
		{
			var client = FixedResponseClient.Create(ProxyAuthResponse, 401,
				contentType: "text/html",
				exception: new Exception("problem with the request as a result of 401")
			);
			var source = await client.LowLevel.GetSourceAsync<GetResponse<Example>>("examples", "example", "1");
			source.Success.Should().BeFalse();
		}

		[U] public async Task BadAuthCarriesStatusCodeAndResponseBodyOverToResponse()
		{
			var client = FixedResponseClient.Create(ProxyAuthResponse, 401,
				contentType: "text/html",
				exception: new Exception("problem with the request as a result of 401")
			);
			var response = await client.LowLevel.GetAsync<GetResponse<Example>>("examples", "example", "1");
			response.Success.Should().BeFalse();
			response.ResponseBodyInBytes.Should().NotBeNullOrEmpty();
			response.HttpStatusCode.Should().Be(401);
		}
	}
}