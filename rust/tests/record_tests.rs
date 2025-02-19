// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

use crate::common::{get_selenium_manager, get_sm_binary};

use kill_tree::blocking::kill_tree;
use std::process::Command;
use std::time::Duration;
use wait_timeout::ChildExt;

mod common;

#[test]
fn test_record() {
    let mut cmd = get_selenium_manager();
    cmd.args(["--browser", "chrome", "--ffmpeg", "--debug"])
        .assert()
        .success()
        .code(0);

    let mut child = Command::new(get_sm_binary())
        .args(["--record", "--debug"])
        .spawn()
        .unwrap();
    let record_time_sec = Duration::from_secs(10);
    let status_code = match child.wait_timeout(record_time_sec).unwrap() {
        Some(status) => status.code(),
        None => {
            kill_tree(child.id()).unwrap();
            child.wait().unwrap().code()
        }
    };
    println!("Recording test status code: {:?}", status_code);
    panic!("Forced error");
}
