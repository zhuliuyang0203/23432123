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

use crate::config::OS::{LINUX, MACOS, WINDOWS};
use crate::downloads::download_to_tmp_folder;
use crate::files::{
    compose_driver_path_in_cache, create_parent_path_if_not_exists, get_filename_with_extension,
    path_to_string, uncompress_tar, unzip,
};
use crate::lock::Lock;
use crate::logger::Logger;
use crate::shell::Command;
use crate::{
    format_four_args, format_one_arg, format_three_args, format_two_args,
    run_shell_command_with_log, ENV_DISPLAY,
};
use anyhow::Error;
use reqwest::Client;
use std::fs::File;
use std::path::{Path, PathBuf};
use std::{env, fs};
use xz2::read::XzDecoder;

pub const FFMPEG_NAME: &str = "ffmpeg";
const FFMPEG_DEFAULT_VERSION: &str = "7.1";
const FFMPEG_WINDOWS_RELEASE_URL: &str =
    "https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{}-essentials_build.7z";
const FFMPEG_LINUX_RELEASE_URL: &str = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-n{}-latest-linux64-gpl-{}.tar.xz";
const FFMPEG_MACOS_RELEASE_URL: &str = "https://evermeet.cx/ffmpeg/ffmpeg-{}.zip";
const FFMPEG_RECORD_FRAME_RATE: &str = "30";
const FFMPEG_RECORD_DESKTOP_WINDOWS_COMMAND: &str = "{} -f gdigrab -i desktop -r {} -q:v 1 -y {}";
const FFMPEG_RECORD_DESKTOP_LINUX_COMMAND: &str = "{} -f x11grab -i {} -r {} -vcodec huffyuv -y {}";
const FFMPEG_RECORD_DESKTOP_MACOS_COMMAND: &str =
    r#"{} -f avfoundation -video_device_index 0 -r {} -y {}"#;
const FFMPEG_RECORDING_EXTENSION_AVI: &str = "avi";
const FFMPEG_RECORDING_EXTENSION_MKV: &str = "mkv";
const FFMPEG_RECORDING_FOLDER: &str = "recordings";
const FFMPEG_DEFAULT_DISPLAY: &str = ":0";

pub fn download_ffmpeg(
    ffmpeg_version: String,
    http_client: &Client,
    os: &str,
    log: &Logger,
    cache_path: PathBuf,
) -> Result<(), Error> {
    let ffmpeg_url = get_ffmpeg_url(os, &ffmpeg_version)?;
    let ffmpeg_path_in_cache = get_ffmpeg_path_in_cache(&ffmpeg_version, os, cache_path)?;
    let ffmpeg_name_with_extension = get_filename_with_extension(FFMPEG_NAME, os);

    let mut lock = Lock::acquire(
        log,
        &ffmpeg_path_in_cache,
        Some(ffmpeg_name_with_extension.clone()),
    )?;
    if !lock.exists() && ffmpeg_path_in_cache.exists() {
        log.debug(format!(
            "{} is already in cache: {}",
            FFMPEG_NAME,
            ffmpeg_path_in_cache.display()
        ));
        return Ok(());
    }

    log.debug(format!(
        "Downloading {} {} from {}",
        FFMPEG_NAME, ffmpeg_version, &ffmpeg_url
    ));
    let (_tmp_folder, driver_zip_file) = download_to_tmp_folder(http_client, ffmpeg_url, log)?;
    uncompress_ffmpeg(
        &driver_zip_file,
        &ffmpeg_path_in_cache,
        log,
        os,
        ffmpeg_name_with_extension,
        &ffmpeg_version,
    )?;

    lock.release();
    Ok(())
}

pub fn get_ffmpeg_version(ffmpeg_version: Option<String>) -> String {
    let ffmpeg_version = ffmpeg_version.unwrap_or(FFMPEG_DEFAULT_VERSION.to_string());
    if ffmpeg_version.is_empty() {
        FFMPEG_DEFAULT_VERSION.to_string()
    } else {
        ffmpeg_version
    }
}

pub fn get_ffmpeg_path_in_cache(
    ffmpeg_version: &str,
    os: &str,
    cache_path: PathBuf,
) -> Result<PathBuf, Error> {
    Ok(compose_driver_path_in_cache(
        cache_path,
        FFMPEG_NAME,
        os,
        get_platform_label(os),
        ffmpeg_version,
    ))
}

fn get_platform_label(os: &str) -> &str {
    if WINDOWS.is(os) {
        "win64"
    } else if MACOS.is(os) {
        "mac-x64"
    } else {
        "linux64"
    }
}

fn get_recording_command(os: &str) -> &str {
    if WINDOWS.is(os) {
        FFMPEG_RECORD_DESKTOP_WINDOWS_COMMAND
    } else if MACOS.is(os) {
        FFMPEG_RECORD_DESKTOP_MACOS_COMMAND
    } else {
        FFMPEG_RECORD_DESKTOP_LINUX_COMMAND
    }
}

fn get_ffmpeg_url(os: &str, ffmpeg_version: &str) -> Result<String, Error> {
    let driver_url = if WINDOWS.is(os) {
        format_one_arg(FFMPEG_WINDOWS_RELEASE_URL, ffmpeg_version)
    } else if MACOS.is(os) {
        format_one_arg(FFMPEG_MACOS_RELEASE_URL, ffmpeg_version)
    } else {
        format_two_args(FFMPEG_LINUX_RELEASE_URL, ffmpeg_version, ffmpeg_version)
    };
    Ok(driver_url)
}

pub fn uncompress_ffmpeg(
    compressed_file: &str,
    target: &Path,
    log: &Logger,
    os: &str,
    ffmpeg_name_with_extension: String,
    ffmpeg_version: &str,
) -> Result<(), Error> {
    if WINDOWS.is(os) {
        // 7z
        let src_path = Path::new(compressed_file);
        let zip_parent = src_path.parent().unwrap();
        sevenz_rust::decompress_file(src_path, zip_parent)?;
        let ffmpeg_binary = format!(
            r"{}\ffmpeg-{}-essentials_build\bin\{}",
            path_to_string(zip_parent),
            ffmpeg_version,
            ffmpeg_name_with_extension
        );
        fs::rename(&ffmpeg_binary, path_to_string(target))?;
    } else if MACOS.is(os) {
        // zip
        unzip(
            compressed_file,
            target,
            log,
            Some(ffmpeg_name_with_extension),
        )?;
    } else {
        // tar.xz
        let src_file = File::open(compressed_file)?;
        let zip_parent = Path::new(compressed_file).parent().unwrap();
        uncompress_tar(&mut XzDecoder::new(src_file), zip_parent, log)?;
        let ffmpeg_binary = format!(
            r"{}/bin/{}",
            path_to_string(zip_parent),
            ffmpeg_name_with_extension
        );
        fs::rename(&ffmpeg_binary, path_to_string(target))?;
    }
    Ok(())
}

fn get_recording_name(os: &str) -> String {
    let now = chrono::Local::now();
    let extension = if MACOS.is(os) {
        FFMPEG_RECORDING_EXTENSION_MKV
    } else {
        FFMPEG_RECORDING_EXTENSION_AVI
    };
    now.format("%Y-%m-%d_%H-%M-%S").to_string() + "." + extension
}

pub fn record_desktop_with_ffmpeg(
    ffmpeg_path: PathBuf,
    os: &str,
    log: &Logger,
    cache_path: PathBuf,
) -> Result<(), Error> {
    let recording_target = cache_path
        .join(FFMPEG_RECORDING_FOLDER)
        .join(get_recording_name(os));
    let recording_name = path_to_string(&recording_target);
    create_parent_path_if_not_exists(&recording_target)?;

    log.debug(format!(
        "Recording desktop with {} to {}",
        FFMPEG_NAME, &recording_name
    ));
    let command = if LINUX.is(os) {
        let env_display = env::var(ENV_DISPLAY).unwrap_or(FFMPEG_DEFAULT_DISPLAY.to_string());
        Command::new_single(format_four_args(
            get_recording_command(os),
            &path_to_string(&ffmpeg_path),
            &env_display,
            FFMPEG_RECORD_FRAME_RATE,
            &recording_name,
        ))
    } else {
        Command::new_single(format_three_args(
            get_recording_command(os),
            &path_to_string(&ffmpeg_path),
            FFMPEG_RECORD_FRAME_RATE,
            &recording_name,
        ))
    };
    run_shell_command_with_log(log, os, command).unwrap();
    Ok(())
}
