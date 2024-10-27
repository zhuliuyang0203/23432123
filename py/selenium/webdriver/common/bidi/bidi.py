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

from dataclasses import dataclass
from dataclasses import fields
from dataclasses import is_dataclass


@dataclass
class BidiObject:
    def to_json(self):
        json = {}
        for field in fields(self):
            key = field.name[1:] if field.name.startswith("_") else field.name
            value = getattr(self, key)
            if value is None:
                continue
            if is_dataclass(value):
                value = value.to_json()
            elif isinstance(value, list):
                value = [v.to_json() if hasattr(v, "to_json") else v for v in value]
            elif isinstance(value, dict):
                value = {k: v.to_json() if hasattr(v, "to_json") else v for k, v in value.items()}
            json[key] = value
        return json

    @classmethod
    def from_json(cls, json):
        return cls(**json)


@dataclass
class BidiEvent(BidiObject):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)


@dataclass
class BidiCommand(BidiObject):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)

    def cmd(self):
        result = yield self.to_json()
        return result
