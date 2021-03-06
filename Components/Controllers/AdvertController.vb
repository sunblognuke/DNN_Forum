﻿'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2011
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Option Strict On
Option Explicit On

Imports DotNetNuke.Services.Vendors

Namespace DotNetNuke.Modules.Forum

    ''' <summary>
    ''' Connects the business layer to the data layer for forum advertisements
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' Bartłomiej Waluszko 12/16/2010 Created
    ''' </history>
    Public Class AdvertController

        ''' <summary>
        '''  Returns a collection of Vendors for a specific moduleID, and vendors not connected to moduleID
        ''' </summary>
        ''' <param name="ModuleID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function VendorsGet(ByVal ModuleID As Integer) As List(Of AdvertInfo)
            Return CBO.FillCollection(Of AdvertInfo)(DataProvider.Instance().VendorsGet(ModuleID))
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="VendorID"></param>
        ''' <param name="IsEnabled"></param>
        ''' <param name="ModuleID"></param>
        ''' <remarks></remarks>
        Public Sub VendorUpdate(ByVal VendorID As Integer, ByVal IsEnabled As Boolean, ByVal ModuleID As Integer)
            DataProvider.Instance().VendorUpdate(VendorID, IsEnabled, ModuleID)
        End Sub

        ''' <summary>
        ''' Get banners list belongs to specified VendorID
        ''' </summary>
        ''' <param name="VendorID"></param>
        ''' <returns></returns>
        ''' <remarks>Replace this by core DNN sproc 'GetBanners' by adding 'ImageFile' field into select statment</remarks>
        Public Function BannersGet(ByVal VendorID As Integer) As List(Of BannerInfo)
            Return CBO.FillCollection(Of BannerInfo)(DataProvider.Instance().Vendors_BannersGet(VendorID))
        End Function

        ''' <summary>
        ''' Increment banner view count
        ''' </summary>
        ''' <param name="BannerID"></param>
        ''' <remarks></remarks>
        Public Sub BannerViewIncrement(ByVal BannerID As Integer)
            DataProvider.Instance().Vendors_BannerViewIncrement(BannerID)
        End Sub
    End Class

End Namespace