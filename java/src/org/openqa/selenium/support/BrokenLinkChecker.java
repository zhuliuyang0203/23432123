package org.openqa.selenium.support;

import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.URI;
import java.net.URL;
import org.openqa.selenium.WebElement;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public final class BrokenLinkChecker {

    private static final Logger logger = LoggerFactory.getLogger(BrokenLinkChecker.class);  // Create a logger instance

    private BrokenLinkChecker() {
        // Utility class - prevent instantiation
    }

    public static boolean isBroken(WebElement element) {
        if (element == null) {
            throw new IllegalArgumentException("Element cannot be null");  // Validate element
        }

        String url = element.getAttribute("href");
        if (url == null || url.trim().isEmpty()) {
            return true;  // Treat empty or null URLs as broken
        }
        return isBroken(url.trim());
    }

    public static boolean isBroken(String linkURL) {
        if (linkURL == null) {
            throw new IllegalArgumentException("Link URL cannot be null");  // Validate linkURL
        }

        try {
            URL url = URI.create(linkURL).toURL();  // This could throw IllegalArgumentException
            HttpURLConnection connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("HEAD");
            connection.setConnectTimeout(5000); // 5 sec timeout
            connection.setReadTimeout(5000);
            connection.connect();

            int responseCode = connection.getResponseCode();
            connection.disconnect();

            return responseCode >= 400;  // Broken link if response code is 400 or higher
        } catch (IOException | IllegalArgumentException e) {
            // Using SLF4J logger for logging the exception
            logger.warn("Error checking link: " + linkURL, e);  // Log the exception with warning level
            return true;  // Return true if any exception occurs (link is considered broken)
        }
    }
}
