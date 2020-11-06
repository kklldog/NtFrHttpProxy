# NtFrHttpProxy
a http ReverseProxy by .net framework 
一个http的反向代理站点。比如A网站在外网，B网站在内外。A无法直接访问通B网站，但是A能够访问堡垒机，堡垒机能够访问B。这个时候在堡垒机上部署这个反向代理软件：
在webconfig，to_server配置成B网站的地址，所有请求到代理软件上的请求全部会转发到to_server上。
