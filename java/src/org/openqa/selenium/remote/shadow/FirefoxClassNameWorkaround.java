package org.openqa.selenium.remote.shadow;

import org.openqa.selenium.By;
import org.openqa.selenium.InvalidSelectorException;
import org.openqa.selenium.SearchContext;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WrapsDriver;
import org.openqa.selenium.remote.HasCapabilities;
import org.openqa.selenium.Capabilities;

final class FirefoxClassNameWorkaround {

    static boolean shouldUseCssSelector(By by, SearchContext context) {
        if (!(by instanceof By.ByClassName)) return false;
        if (!(context instanceof WrapsDriver)) return false;

        try {
            WebDriver driver = ((WrapsDriver) context).getWrappedDriver();
            Capabilities caps = ((HasCapabilities) driver).getCapabilities();
            return "firefox".equalsIgnoreCase(caps.getBrowserName());
        } catch (Exception ignored) {
            return false;
        }
    }

    static By convertToCss(By by) {
        String className = extractClassName(by);
        if (className.contains(" ")) {
            throw new InvalidSelectorException(
                "Compound class names not supported in Firefox Shadow DOM. Use single class name or By.cssSelector instead."
            );
        }
        return By.cssSelector("." + className);
    }

    private static String extractClassName(By by) {
        String raw = by.toString().replace("By.className:", "").trim();
        return raw.replaceAll("\\s+", "");
    }

    public static By resolveForShadow(By by, SearchContext context) {
        return shouldUseCssSelector(by, context) ? convertToCss(by) : by;
    }
}
