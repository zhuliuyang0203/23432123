# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.

import typing
from dataclasses import dataclass

from selenium.webdriver.common.bidi.cdp import import_devtools

from . import browsing_context
from . import script
from .bidi import BidiCommand
from .bidi import BidiEvent
from .bidi import BidiObject

devtools = import_devtools("")
event_class = devtools.util.event_class

InterceptPhase = typing.Literal["beforeRequestSent", "responseStarted", "authRequired"]


@dataclass
class UrlPatternPattern(BidiObject):
    _type: typing.Literal["pattern"] = "pattern"
    protocol: typing.Optional[str] = None
    hostname: typing.Optional[str] = None
    port: typing.Optional[str] = None
    pathname: typing.Optional[str] = None
    search: typing.Optional[str] = None


@dataclass
class UrlPatternString(BidiObject):
    pattern: str
    _type: typing.Literal["string"] = "string"


UrlPattern = typing.Union[UrlPatternPattern, UrlPatternString]


@dataclass
class AddInterceptParameters(BidiObject):
    phases: typing.List[InterceptPhase]
    contexts: typing.Optional[typing.List[browsing_context.BrowsingContext]] = None
    urlPatterns: typing.Optional[typing.List[UrlPattern]] = None


@dataclass
class AddIntercept(BidiCommand):
    params: AddInterceptParameters
    method: typing.Literal["network.addIntercept"] = "network.addIntercept"


Request = str


@dataclass
class StringValue(BidiObject):
    value: str
    _type: typing.Literal["string"] = "string"


@dataclass
class Base64Value(BidiObject):
    value: str
    _type: typing.Literal["base64"] = "base64"


BytesValue = typing.Union[StringValue, Base64Value]


@dataclass
class Header(BidiObject):
    name: str
    value: BytesValue


SameSite = typing.Literal["strict", "lax", "none"]


@dataclass
class Cookie(BidiObject):
    name: str
    value: BytesValue
    domain: str
    path: str
    size: int
    httpOnly: bool
    secure: bool
    sameSite: SameSite
    expiry: typing.Optional[int] = None


@dataclass
class FetchTimingInfo(BidiObject):
    timeOrigin: float
    requestTime: float
    redirectStart: float
    redirectEnd: float
    fetchStart: float
    dnsStart: float
    dnsEnd: float
    connectStart: float
    connectEnd: float
    tlsStart: float
    requestStart: float
    responseStart: float
    responseEnd: float


@dataclass
class RequestData(BidiObject):
    request: Request
    url: str
    method: str
    headersSize: int
    timings: FetchTimingInfo
    headers: typing.Optional[typing.List[Header]] = None
    cookies: typing.Optional[typing.List[Cookie]] = None
    bodySize: typing.Optional[int] = None


Intercept = str


@dataclass
class Initiator(BidiObject):
    _type: typing.Literal["parser", "script", "preflight", "other"]
    columnNumber: typing.Optional[int] = None
    lineNumber: typing.Optional[int] = None
    stackTrace: typing.Optional[script.StackTrace] = None
    request: typing.Optional[Request] = None


@dataclass
class BeforeRequestSentParameters(BidiObject):
    isBlocked: bool
    redirectCount: int
    request: RequestData
    timestamp: int
    initiator: Initiator
    context: typing.Optional[browsing_context.BrowsingContext] = None
    navigation: typing.Optional[browsing_context.Navigation] = None
    intercepts: typing.Optional[typing.List[Intercept]] = None


@dataclass
@event_class("network.beforeRequestSent")
class BeforeRequestSent(BidiEvent):
    params: BeforeRequestSentParameters
    method: typing.Literal["network.beforeRequestSent"] = "network.beforeRequestSent"

    param_class = BeforeRequestSentParameters


@dataclass
class CookieHeader(BidiObject):
    name: str
    value: BytesValue


@dataclass
class ContinueRequestParameters(BidiObject):
    request: Request
    body: typing.Optional[BytesValue] = None
    cookies: typing.Optional[typing.List[CookieHeader]] = None
    headers: typing.Optional[typing.List[Header]] = None
    method: typing.Optional[str] = None
    url: typing.Optional[str] = None


@dataclass
class ContinueRequest(BidiCommand):
    params: ContinueRequestParameters
    method: typing.Literal["network.continueRequest"] = "network.continueRequest"


@dataclass
class RemoveInterceptParameters(BidiObject):
    intercept: Intercept


@dataclass
class RemoveIntercept(BidiCommand):
    params: RemoveInterceptParameters
    method: typing.Literal["network.removeIntercept"] = "network.removeIntercept"


@dataclass
class SetCacheBehaviorParameters(BidiObject):
    cacheBehavior: typing.Literal["default", "bypass"]
    contexts: typing.Optional[typing.List[browsing_context.BrowsingContext]] = None


@dataclass
class SetCacheBehavior(BidiCommand):
    params: SetCacheBehaviorParameters
    method: typing.Literal["network.setCacheBehavior"] = "network.setCacheBehavior"


class Network:
    def __init__(self, conn):
        self.conn = conn

    async def add_intercept(self, params: AddInterceptParameters):
        result = await self.conn.execute(AddIntercept(params).cmd())
        return result

    async def continue_request(self, params: ContinueRequestParameters):
        result = await self.conn.execute(ContinueRequest(params).cmd())
        return result

    async def remove_intercept(self, params: RemoveInterceptParameters):
        await self.conn.execute(RemoveIntercept(params).cmd())
