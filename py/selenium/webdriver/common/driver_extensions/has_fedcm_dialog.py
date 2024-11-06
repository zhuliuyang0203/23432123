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
from selenium.webdriver.common.fedcm.dialog import Dialog
from selenium.webdriver.remote.webdriver import WebDriver


class HasFedCmDialog(WebDriver):
    """Mixin that provides FedCM-specific functionality."""

    @property
    def dialog(self):
        """Returns the FedCM dialog object for interaction."""
        return Dialog(self)

    def enable_fedcm_delay(self):
        """Re-enables the promise rejection delay for FedCM."""
        self.fedcm.enable_delay()

    def disable_fedcm_delay(self):
        """Disables the promise rejection delay for FedCM.

        FedCm by default delays promise resolution in failure cases for
        privacy reasons. This method allows turning it off to let tests
        run faster where this is not relevant.
        """
        self.fedcm.disable_delay()

    def fedcm_cooldown(self):
        """Resets the FedCm dialog cooldown.

        If a user agent triggers a cooldown when the account chooser is
        dismissed, this method resets that cooldown so that the dialog
        can be triggered again immediately.
        """
        self.fedcm.reset_cooldown()

    def fedcm_dialog(self, timeout=5, poll_frequency=0.5, ignored_exceptions=None):
        """Waits for the FedCM dialog to appear."""
        from selenium.common.exceptions import NoAlertPresentException
        from selenium.webdriver.support.wait import WebDriverWait

        if ignored_exceptions is None:
            ignored_exceptions = (NoAlertPresentException,)

        def _check_fedcm():
            try:
                return self.dialog if self.dialog.type else None
            except NoAlertPresentException:
                return None

        wait = WebDriverWait(self, timeout, poll_frequency=poll_frequency, ignored_exceptions=ignored_exceptions)
        return wait.until(lambda _: _check_fedcm())
