﻿// Copyright (c) 2008 Madgex
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// OAuth.net uses the Common Service Locator interface, released under the MS-PL
// license. See "CommonServiceLocator License.txt" in the Licenses folder.
// 
// The examples and test cases use the Windsor Container from the Castle Project
// and Common Service Locator Windsor adaptor, released under the Apache License,
// Version 2.0. See "Castle Project License.txt" in the Licenses folder.
// 
// XRDS-Simple.net uses the HTMLAgility Pack. See "HTML Agility Pack License.txt"
// in the Licenses folder.
//
// Authors: Bruce Boughton, Chris Adams
// Website: http://lab.madgex.com/oauth-net/
// Email:   oauth-dot-net@madgex.com

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.UI;
using OAuth.Net.Examples.TwitterClient.Api;

namespace OAuth.Net.Examples.TwitterClient
{
    public partial class UpdateStatusPage : Page
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Catches exception to hide it from end user")]
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings",
            Justification = "Spurious warning")]
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!"POST".Equals(this.Request.HttpMethod, StringComparison.OrdinalIgnoreCase))
                throw new HttpException(405, "Method not allowed");

            string status = this.Request.Params["status"];

            if (string.IsNullOrEmpty(status))
                throw new HttpException(400, "Bad Request");

            // Create Twitter API
            TwitterApi api = new TwitterApi(
                new ApiCallOptions
                {
                    AuthorizationCallbackUri = new Uri(
                        this.Request.Url,
                        new VPathResolver().Resolve(
                            "~/UpdateStatus.aspx?status=" + status))
                });

            try
            {
                // Call Twitter API
                api.UpdateStatus(status);
            }
            catch (BadCredentialsException)
            {
                this.Response.Redirect("~/Disconnect.aspx?error=badcredentials", true);
            }
            catch (Exception)
            {
                // The API call didn't work out (or something else bad happened)
                this.Response.Redirect("~/User.aspx?error=unknown", true);
            }

            // Update must have been successful
            // Force reload of affected caches
            this.Session.Remove(UserPage.UserInfoKey);
            this.Session.Remove(UserPage.UserTimelineKey);

            // Redirect to User page
            this.Response.Redirect("~/User.aspx", true);
        }
    }
}
