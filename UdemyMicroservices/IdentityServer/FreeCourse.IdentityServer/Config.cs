// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace FreeCourse.IdentityServer
{
    public static class Config
    {
        //bunlar metot değiller get propertiysler => ile beraber.
        public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
        {
         //tanımladığım scopeları kim kullanacak belirtelim. Kaç tane microservisimiz varsa o kadar API Resource olacak.
         //Audlarımı burada dolduracağım token içerisindeki ve bu resource isimlerini microsevislerimin startupındaki ayarlamada Audience kısmında kullancağım. Microservisim ordan anlayacak resource_basket olduğunu buna göre buraya istek yapacak. Yani microsversite resource_basket dediğim için istek burada resource_basket için çalışacak ve bu resourcenin scopelarına göre 2 client türümden hangisine gitmesi gerektiğini anlayacak.
            new ApiResource("resource_catalog"){Scopes={"catalog_fullpermission"}},
            new ApiResource("resource_photo_stock"){Scopes={"photo_stock_fullpermission"}},
            new ApiResource("resource_basket"){Scopes={"basket_fullpermission"}},
            new ApiResource("resource_discount"){Scopes={"discount_fullpermission"}},
            new ApiResource("resource_order"){Scopes={"order_fullpermission"}},
            new ApiResource("resource_payment"){Scopes={"payment_fullpermission"}},
            new ApiResource("resource_gateway"){Scopes={"gateway_fullpermission"}},
            //Identity serverin kendisi için var
            new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
        };

        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                       //kullanıcıyla ilgili işlemler
                       //yani burada izin veriyoruz kullanıcının emailine erişebilsin diye bu izin sadece içinde data yok karıştırma, Identity serverin User info endpointi ile bu bilgilere ulaşabiliyoruz bu endpointe tokeni vermemiz gerekir.
                       //default gelen bazı resourcesler var onları kullanıcaz
                       //clientler kullanıcının emailine erişebilir. token payload içindeki scope alanında email claimine karşılık geliyor.
                       new IdentityResources.Email(),
                       //jwt tokenın payloadındaki sub keywordunun mutlaka dolu olması lazım yani openID mutlaka olmak durumunda zorunlu. Kullanıcının ıdsine ulaşabilsin. token payloadındaki sub claimine karşılık geliyor.
                       new IdentityResources.OpenId(),
                       //kullanıcının profil bilgileri varsa onlara erişebilsin. yine payloaddaki scope kısmına geliyor
                       new IdentityResources.Profile(),
                       //rollerede erişebilsin, kendim claim maplemem lazım hazır gelmiyor, role ismindeki claim'e maplansın bu diyorum. yine payloaddaki scope kısmına geliyor.
                       new IdentityResource(){Name="roles", DisplayName="Roles", Description="Kullanıcı rolleri", UserClaims= new []{"role"}}
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                //tokenin içindeki scopelarını burda tanımlayalım
                new ApiScope("catalog_fullpermission","Catalog API için full erişim"),
                new ApiScope("photo_stock_fullpermission","Photo Stock API için full erişim"),
                new ApiScope("basket_fullpermission","Basket API için full erişim"),
                new ApiScope("discount_fullpermission","Discount API için full erişim"),
                new ApiScope("order_fullpermission","Order API için full erişim"),
                new ApiScope("payment_fullpermission","Payment API için full erişim"),
                new ApiScope("gateway_fullpermission","Gateway API için full erişim"),
                //birde ıdentityserverin kendisi için
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
            };

        //scopelarımı tanımladım sonra resourcelarımıda tanımladıktan sonra artık 2 tür clientim olacak bir login işlemsiz token isteği yapan client birde login işlemli email password gönderen token isteği yapan client. Resourcelarımı yani microservislerimi buna göre bu 2 clienttan birine ekliyorum. Burada ilk clientim login işlemi gerektirmeyen microservislerim için. Allowscopes parametresinde bu microsevislerimi belirticem bunlar istek yapabilsin diyeceğim.
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                //clientımı burada tanımlayacağım
                new Client
                {
                    //client için kullanıcı adı ve passwordu gibi düşünebiliriz bunları, eğer merkezi bir üyelik sistemi kullansaydım yani kullanıcı login veya register olmak istediğinde IdentityServerın arayüzüne yönlendirseydim orada şu client senden şu bilgileri istiyor işte o clientnamei burdan alıyor. Bizim için gerek yok ama yinede tanımlayalım.
                    //bu şifreyi hashlemem lazım Sha256 ile beraber ,bu secreti istersek db de tutabiliriz şuanlık bu bilgiler memoryde olacak ama istersem bu bilgielri tamamen db den çekebilriim.
                    ClientName="Asp.Net Core MVC",
                    ClientId="WebMvcClient",
                    ClientSecrets={new Secret("secret".Sha256())},
                    //üyelik sistemi gerektirmeyen akış tipini belirtelim. Bu akış tipi ile beraber reflesh token olmaz çünkü zaten elimizde sabit bir clientid clientsecret değeri var zaten her zaman sen gidip token alabilirsin ama user işin içine girdiğinde elinde username password veya email password yoksa her zaman token alamazsın işte refleshtoken orada devreye giriyor.
                    AllowedGrantTypes=GrantTypes.ClientCredentials,
                    //izin verilen scopeları belirtelim, sonuçta bu clientid, clientsecret istek yaptığı zaman işte burdaki scopelara göre IdentityServer bir token üretecek. Buraya microservislerimizi veriyoruz birde kendisinede istek yapabilir diyoruz yani local api ekliyoruz.
                    AllowedScopes={ "catalog_fullpermission", "photo_stock_fullpermission", "gateway_fullpermission", IdentityServerConstants.LocalApi.ScopeName }
                },
                //şimdi ResourceOwnerCredentialsta clientlar kullanıcı bilgilerinden tanımladıklarımızdan hangilerine erişebilecekler.
                new Client
                {
                    ClientName="Asp.Net Core MVC",
                    ClientId="WebMvcClientForUser",
                    //refleshtokena izin vermem lazım
                    AllowOfflineAccess = true,
                    ClientSecrets={new Secret("secret".Sha256())},
                    //burada ResourceOwnerPasswordAndClientCredentials kullanırsak reflesh token alamayız.
                    AllowedGrantTypes=GrantTypes.ResourceOwnerPassword,
                    //tokenla beraber kullanıcın tüm verilerine erişebilsin clientlar, openid mutlaka olmalı zorunlu. Ayrıca ben her token üretimiyle beraber birde reflesh tokenda vermek istiyorum. O yüzden OfflineAccess de veriyoruz. Reflesh token elimizde varsa kullanıcı o an login olmasa dahi biz kullanıcı adına reflesh tokeni gönderip kullanıcı adına tekrar bir token elde edebiliriz o yüzden ismi offline yani kullanıcı offline olsa dahi ben kullanıcı adına elimdeki reflesh token ile beraber yeni bir token alabilrim o yüzden ismi offline. Eğer bunu yazmazsam elimde refles token olmazsa ben yeni bir token almak istediğimde mutlaka kullanıcıdan email ve password almam gerekiyor. Yani kullanıcının aldığı tokenin ömrü 1 saat sonra dolarsa ben kullanıcıyı yine login ekranına döndürmem gerekiyor kullanıcı tekrar email şifre yazsın öyle tekrar token alsın burada 2 türlü çözüm olabilir bir access tokenin alınan tokenin ömrü uzatılabilir bu iyi bir şey değil. İki elimde reflesh yoken olursa accesstokenin ömrünü 1 saat yaparım reflestokenin ömrünü 60 gün yaparım eğerki eldeki mevcut tokenin ömrü dolarsa elimde zaten cookiede tuttuğum reflesh token var hiç kullanıcya hissettirmeden accesstokena istek yaptım 401 aldım elimde refleshtoken var bununla git ıdentityserverden yeni bir token al yine aynı kullanıcıyla istek yap ve bunu bu sayede hiç kullanıcıya hissettirmemiş ol. IdentityServerConstants.LocalApi.ScopeName ile bu local apiya yani IdentityServer microservisinin kendisinede istek yapabilir diyoruz.
                    //Userinfo endpointinin kullanıcı hakkında bize hangi verileri getireceğinin bilgisini burada vermiş oluyoruz email bilgisini getirsin gibi.
                    AllowedScopes={ "basket_fullpermission", "order_fullpermission", "gateway_fullpermission", IdentityServerConstants.StandardScopes.Email, IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, IdentityServerConstants.StandardScopes.OfflineAccess, IdentityServerConstants.LocalApi.ScopeName, "roles"},
                    //accesstokunun ömrünü 1 saat yapabiliriz.
                    AccessTokenLifetime= 1*60*60,
                    //refleshtokenın exprirationunu  ve ömrünü belirtelim, bu refleshtokena kesin bir tarih mi vericem yoksa Slinding mi yani reflesh token istedikçe ömrünü arttıracak mıyım. Kesin bir tarih verelim 60 günlük olsun yani 61. günde artık o reflesh token ile beraber yeni bir token alınamasın.
                    RefreshTokenExpiration= TokenExpiration.Absolute,
                    AbsoluteRefreshTokenLifetime = (int)(DateTime.Now.AddDays(60)-DateTime.Now).TotalSeconds,
                    //refleshtoken bir kere mi kullanılsın yoksa arka arkaya kullanılabilir mi, Tekrar kullanılabilir olsun
                    RefreshTokenUsage = TokenUsage.ReUse,
                    //bu arada kullanıcı her yeni token aldığında yeni bir 60 günlük reflesh tokenda almış olacak her seferinde. Ama bir kere token aldı girdi refleshtokenda aldı ama 60 gün boyunca hiç girmedi 61. gün artık accesstokenında refleshtokeninde ömrü dolacak baştan login olması gerekecek.
                },
                //tokenexchange için yeni client
                //resourceowner için olan clienttan scopelarından "discount_fullpermission", "payment_fullpermission" kaldıralım
                //bu sayede kullanıcı resource owner token aldığında bunlara istek yapamasın
                //birde kullanıcının idsi olması lazım IdentityServerConstants.StandardScopes.OpenId ile
                //zaten gateway bu client ile beraber IdentityServera istek yapacak onu eklemeye gerek yok.
                //resourceowner scopelarındaki izinlere sahip tokeni ve buradaki clienttaki scoeplardaki izinlere sahip tokeni al amacımız bu.
                new Client
                {
                    ClientName="Token Exchange Client",
                    ClientId="TokenExhangeClient",
                    ClientSecrets={new Secret("secret".Sha256())},
                    AllowedGrantTypes=new []{"urn:ietf:params:oauth:grant-type:token-exchange"},
                    AllowedScopes={ "discount_fullpermission", "payment_fullpermission", IdentityServerConstants.StandardScopes.OpenId }
                },
            };
    }
}