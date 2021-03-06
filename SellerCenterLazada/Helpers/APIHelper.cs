﻿using Newtonsoft.Json;
using SellerCenterLazada.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SellerCenterLazada
{
    public class APIHelper
    {
        const string LOGIN_URL = "https://m.sellercenter.lazada.vn/m/seller/login.html";
        const string COOKIE_URL = "https://uac.lazada.com/tbpass/jump?group=lazada-seller&target=" + LOGIN_URL;
        const string USER_INFO_URL = "https://m.sellercenter.lazada.vn/m/index/info";
        // Thông tin mới
        const string PRODUCTS_URL = "https://nest.sellercenter.lazada.vn/api/gateway?__gateway_method_id=api.get.contentGenerator.getShopNewArrivalProducts&pageSize={0}&pageNum={1}";
        // Mã giảm giá
        const string VOUCHERS_URL = "https://nest.sellercenter.lazada.vn/api/gateway?__gateway_method_id=api.get.contentGenerator.getVoucherList&ua=&umid=";
        // Deal nổi bật
        const string CAMPAIGN_URL = "https://nest.sellercenter.lazada.vn/api/gateway?__gateway_method_id=api.get.contentGenerator.getShopCampaignList";
        const string CAMPAIGN_PRODUCTS_URL = "https://nest.sellercenter.lazada.vn/api/gateway?__gateway_method_id=api.get.contentGenerator.getCampaignProducts&pageSize=20&pageNum=1&campaignId={0}";
        // Ảnh từ khách hàng
        const string REVIEWS_CUSTOMER_URL = "https://sellercenter.lazada.vn/asc-review/product-reviews/getList?pageSize=10&pageNo=1&contentType=with_images&productName=&orderNumber=&sellerSku=&shopSku=&fromDate=&toDate=&replyState=&rating=5&sourceAppName=&isExternal=false";
        // Đăng dạo
        // const string CHECK_FEED_URL = "https://nest.sellercenter.lazada.vn/api/gateway?__gateway_method_id=api.post.contentGenerator.checkFeedDesc";
        const string CREATE_FEED_URL = "https://nest.sellercenter.lazada.vn/api/gateway?__gateway_method_id=api.post.contentGenerator.createFeed";
        // Đăng dạo reviews
        //https://sellercenter.lazada.vn/asc-review/seller-manage-reviews/updateShareStatus , post data = "type=feed&reviewRateId=228442819291426&isActive=true"
        // Phân tích bán hàng nâng cao sản phẩm
        const string PRODUCT_SALES_ANALYSIS_VIEW_URL = "https://m.sellercenter.lazada.vn/sycm/lazada/product/performance/mobile/realtime/rank.json?indexCode=uv&pageSize={0}&page={1}&brandId=&cateId=&lang=&_sycm_wl_=1";
        const string PRODUCT_SALES_ANALYSIS_URL = "https://m.sellercenter.lazada.vn/sycm/lazada/mobile/dashboard/product/rankList/dateRange.json?indexCode=uv&pageSize={0}&page={1}&dateType=day&dateRange={2}&brandId=&cateId=&lang=&_sycm_wl_=1";



        public static string cookie = "";
        public static bool Login(string username, string password)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(COOKIE_URL);
                request.Method = "GET";
                request.AllowAutoRedirect = false;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/66.4.120 Chrome/60.4.3112.120 Safari/537.36";
                request.Headers.Add("Accept-Language:vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                cookie = Regex.Replace(response.GetResponseHeader("Set-Cookie"), "( Domain=.+?;)|( Path=.{0,2})|( HttpOnly,)", "");
                request = (HttpWebRequest)WebRequest.Create(LOGIN_URL);
                request.Method = "POST";
                request.AllowAutoRedirect = false;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/66.4.120 Chrome/60.4.3112.120 Safari/537.36";
                request.Headers.Add("Accept-Language:vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2");
                request.Headers.Add("Cookie", cookie);
                var postData = $"TPL_username={HttpUtility.UrlEncode(username)}&TPL_password={HttpUtility.UrlEncode(password)}";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Flush();
                dataStream.Close();
                response = (HttpWebResponse)request.GetResponse();
                return HttpStatusCode.Found.Equals(response.StatusCode);
            }
            catch (Exception e)
            {
                Console.Write(e);
                return false;
            }
        }
        public static UserInfo GetUserInfo()
        {
            try
            {
                var data = Get(USER_INFO_URL);
                return JsonConvert.DeserializeObject<UserInfo>(data);
            }
            catch (Exception e)
            {
                Console.Write(e);
                return null;
            }
        }
        public static List<ProductInfoVoList> GetProductInfoVoList()
        {
            List<ProductInfoVoList> productInfoVos = new List<ProductInfoVoList>();
            try
            {
                int page = 1;
                ProductInfoVo productInfo = null;
                List<ProductInfoVoList> productInfoVos1 = null;
                while (true)
                {
                    var data = Get(string.Format(PRODUCTS_URL, 20, page));
                    productInfo = JsonConvert.DeserializeObject<ProductInfoVo>(data);
                    productInfoVos1 = productInfo.result.productInfoVoList.FindAll(p => p.feedStatus == 0);
                    productInfoVos.AddRange(productInfoVos1);
                    if (productInfo.result.productInfoVoList.Count < 20)
                    {
                        break;
                    }
                    page++;
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
            return productInfoVos;
        }

        public static ProductSalesAnalysis GetProductSalesAnalysis(int pageSize = 100, int pageNum = 1, string dateRange = "")
        {
            var data = Get(string.Format(PRODUCT_SALES_ANALYSIS_URL, pageSize, pageNum, dateRange));
            return JsonConvert.DeserializeObject<ProductSalesAnalysis>(data);
        }

        public static string CreateFeed(ProductInfoVoList productInfoVo)
        {
            try
            {
                //if (productInfoVo.title.Length > 100)
                //{
                //    productInfoVo.title = productInfoVo.title.Substring(0, 90) + "...";
                //}
                List<FeedContent> feedContent = new List<FeedContent>();
                feedContent.Add(new FeedContent(
                    productInfoVo.skuId,
                    productInfoVo.itemId,
                    productInfoVo.imageUrl
                ));
                FeedDesc feedDesc = new FeedDesc();
                feedDesc.vi = new ViEn(
                    productInfoVo.title,
                    productInfoVo.title + "\n" + "Giá " + productInfoVo.discountPriceFormatted
                );
                feedDesc.en = feedDesc.vi;
                Feed feed = new Feed();
                feed.feedContent = feedContent;
                feed.feedDesc = feedDesc;
                var data = Post(CREATE_FEED_URL, JsonConvert.SerializeObject(feed));
                return data;
            }
            catch (Exception e)
            {
                Console.Write(e);
                return null;
            }
        }
        private static string Get(string URL, bool isAllowRedirect = false)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";
                request.AllowAutoRedirect = isAllowRedirect;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/66.4.120 Chrome/60.4.3112.120 Safari/537.36";
                request.Headers.Add("Upgrade-Insecure-Requests:1");
                request.Headers.Add("Accept-Language:vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2");
                request.Headers.Add("Cookie", cookie);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (!HttpStatusCode.OK.Equals(response.StatusCode)) return null;
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                var result = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(result)) return null;
                reader.Close();
                responseStream.Close();
                response.Close();
                return result;
            }
            catch (Exception e)
            {
                Console.Write(e);
                return null;
            }
        }
        private static string Post(string URL, string postData, bool isAllowRedirect = false)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "POST";
                request.AllowAutoRedirect = isAllowRedirect;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/66.4.120 Chrome/60.4.3112.120 Safari/537.36";
                request.ContentType = "application/json";
                request.Headers.Add("Upgrade-Insecure-Requests: 1");
                request.Headers.Add("Accept-Language:vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2");
                request.Headers.Add("Cookie", cookie);
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Flush();
                dataStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (!HttpStatusCode.OK.Equals(response.StatusCode)) return null;
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                var result = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(result)) return null;
                reader.Close();
                responseStream.Close();
                response.Close();
                return result;
            }
            catch (Exception e)
            {
                Console.Write(e);
                return null;
            }
        }
    }
}
