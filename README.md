# NtFrHttpProxy
a http ReverseProxy by .net framework    

一个http的反向代理站点。比如A网站在外网，B网站在内外。A无法直接访问通B网站，但是A能够访问堡垒机，堡垒机能够访问B。这个时候在堡垒机上部署这个反向代理软件：
在webconfig，to_server配置成B网站的地址，所有请求到代理软件上的请求全部会转发到to_server上。    
至于为什么这年头了还要用framework来实现这个代理，以为.net core发出的请求在某些情况下会带有一个request-id的头部，如果对方服务器防火墙策略暴力，会把这个请求直接拒绝。然后core没办法完全关闭这个莫名其妙的头部，所以无奈只能用framework来实现。
