using Microsoft.AspNetCore.WebUtilities;
using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;
using Radzen;
using System.Collections;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Mimeo.DynamicUI.Extensions
{
    /// <summary>
    /// Extensions intended to replicate the feel of FlurlClient, but without as much infrastructure
    /// </summary>
    public static class HttpClientExtensions
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static HttpClientBaseRequest Request(this HttpClient httpClient, string endpoint)
        {
            return new HttpClientBaseRequest
            {
                HttpClient = httpClient,
                Endpoint = httpClient.BaseAddress != null ? new Uri(httpClient.BaseAddress, endpoint) : new Uri(endpoint)
            };
        }

        public static HttpClientBaseRequest Request(this HttpClient httpClient, Uri endpoint)
        {
            return new HttpClientBaseRequest
            {
                HttpClient = httpClient,
                Endpoint = endpoint
            };
        }

        public static async Task<DataResponse<TModel>> QueryOData<TModel, TWrapper>(this HttpClientBaseRequest request, Func<TWrapper, DataResponse<TModel>> mapper, string? filter = null, int? top = null, int? skip = null, string? orderby = null, string? expand = null, string? select = null, bool? count = null, Func<Uri, Uri>? urlCustomizer = null)
        {
            var odataUri = request.Endpoint.GetODataUri(filter: filter, top: top, skip: skip, orderby: orderby, expand: expand, select: select, count: count);

            if (urlCustomizer != null)
            {
                odataUri = urlCustomizer(odataUri);
            }

            var response = await request.HttpClient.GetAsync(odataUri);
#if DEBUG
            if (!response.IsSuccessStatusCode)
            {
                var rawResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(rawResponse);
            }
#endif
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TWrapper>(JsonSerializerOptions) ?? throw new Exception("Failed to deserialize response");
            return mapper(result);
        }

        public static Task<DataResponse<TModel>> QueryOData<TModel, TWrapper>(this HttpClientBaseRequest request, Func<TWrapper, DataResponse<TModel>> mapper, LoadDataArgs args, Func<Uri, Uri>? urlCustomizer = null)
        {
            return QueryOData<TModel, TWrapper>(request, mapper, args.Filter, args.Top, args.Skip, args.OrderBy, count: true, urlCustomizer: urlCustomizer);
        }

        public static Task<DataResponse<TModel>> QueryOData<TModel, TWrapper>(this HttpClientBaseRequest request, Func<TWrapper, DataResponse<TModel>> mapper, DataQuery query, ODataExpressionGenerator oDataExpressionGenerator, Func<Uri, Uri>? urlCustomizer = null)
        {
            return QueryOData<TModel, TWrapper>(request, mapper, filter: oDataExpressionGenerator.GenerateODataFilter(query), orderby: oDataExpressionGenerator.GenerateODataOrderBy(query), skip: query.Skip, top: query.Top, count: true, urlCustomizer: urlCustomizer);
        }

        public static async Task<HttpResponseMessage> GetAsync(this HttpClientBaseRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, request.Endpoint);
            var response = await request.HttpClient.SendAsync(requestMessage);
            return response;
        }

        public static async Task<HttpResponseMessage> GetAsync(this HttpClientBaseRequest request, object queryString)
        {
            var uri = BuildQueryString(request.Endpoint, queryString);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await request.HttpClient.SendAsync(requestMessage);
            return response;
        }

        public static async Task<TResponse?> GetJsonAsync<TResponse>(this HttpClientBaseRequest request, bool returnDefaultOn404 = false)
        {
            var response = await request.GetAsync();
            if (returnDefaultOn404 && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsJsonAsync<TResponse>();
        }

        public static async Task<TResponse?> GetJsonAsync<TResponse>(this HttpClientBaseRequest request, object queryString, bool returnDefaultOn404 = false)
        {
            var response = await request.GetAsync(queryString);
            if (returnDefaultOn404 && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsJsonAsync<TResponse>();
        }

        public static async Task<HttpResponseMessage> SendJsonAsync(this HttpClientBaseRequest request, HttpMethod method, object requestBody)
        {
            if (requestBody == null)
            {
                throw new ArgumentNullException(nameof(requestBody));
            }

            var serialized = JsonSerializer.Serialize(requestBody, requestBody.GetType(), JsonSerializerOptions);

            var response = await request.HttpClient.SendAsync(new HttpRequestMessage(method, request.Endpoint)
            {
                Content = new StringContent(serialized, Encoding.UTF8, "application/json")
            });
#if DEBUG
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Encountered {response.StatusCode} from {request.Endpoint}. Body: {await response.Content.ReadAsStringAsync()}");
            }
#endif
            response.EnsureSuccessStatusCode();
            return response;
        }

        public static async Task<HttpResponseMessage> PostJsonAsync(this HttpClientBaseRequest request, object requestBody) => await request.SendJsonAsync(HttpMethod.Post, requestBody);
        public static async Task<HttpResponseMessage> PutJsonAsync(this HttpClientBaseRequest request, object requestBody) => await request.SendJsonAsync(HttpMethod.Put, requestBody);

        public static async Task<HttpResponseMessage> DeleteAsync(this HttpClientBaseRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, request.Endpoint);
            var response = await request.HttpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public static async Task<T?> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var stringContent = await content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(stringContent, JsonSerializerOptions);
        }

        public static string BuildQueryString(Uri uri, object queryStringParameters)
        {
            return BuildQueryString(uri.ToString(), queryStringParameters);
        }

        public static string BuildQueryString(string uri, object queryStringParameters)
        {
            var parameters = new Dictionary<string, string?>();
            foreach (var property in queryStringParameters.GetType().GetProperties())
            {
                var value = property.GetValue(queryStringParameters);
                if (value != null)
                {
                    parameters.Add(property.Name, value.ToString());
                }
            }

            return QueryHelpers.AddQueryString(uri, parameters);
        }

        public static string ToQueryStringValue(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            if (obj is IEnumerable enumerable)
            {
                var value = new StringBuilder();
                foreach (var item in enumerable)
                {
                    var itemString = item.ToQueryStringValue();
                    value.Append(itemString);
                    value.Append(',');
                }
                if (value.Length > 0)
                {
                    value.Length -= 1;
                }
                return value.ToString();
            }

            return obj.ToString()!;
        }

        public struct HttpClientBaseRequest
        {
            public HttpClient HttpClient { get; set; }
            public Uri Endpoint { get; set; }
        }
    }
}
