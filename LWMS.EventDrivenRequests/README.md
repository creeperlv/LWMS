
# EDR - Event Driven Requests

This module is designed for dealing http(s) requests in en event-driven way instead of pipeline which could lag the server as working process units grows.

## Event driven, how?

This module will read its configuration and find all registered url prefixes (e.g.: when receieved a url `/foo/bar.html`, `foo/` will be an legal prefix), then load all handlers in the first request. Also, EDR module uses SBSDomain, so DLLs of the handlers can be dynamically updated.

This module will determine the which handler to use and deliver the request and context to the target handler.

## Register a handler

To register a handler, you need to enable the manage command from `EDRMgr`, then, register the handler by command `edrmgr --r <URL-Prefix> <DLL-File> <Namespace.Type>`

## Remove a handler

Simply use the command `edrmgr --rm <URL-Prefix>`