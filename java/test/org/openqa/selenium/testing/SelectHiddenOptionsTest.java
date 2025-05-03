package org.example;

import io.github.bonigarcia.wdm.WebDriverManager;
import org.openqa.selenium.By;
import org.openqa.selenium.NoSuchElementException;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;
import org.openqa.selenium.support.ui.Select;
import java.util.List;
import java.util.Scanner;

public class SelectHiddenOptionsTest {
    public static void main(String[] args) {
        WebDriverManager.chromedriver().setup();
        ChromeOptions options = new ChromeOptions();
        options.addArguments("--remote-allow-origins=*");
        WebDriver driver = new ChromeDriver(options);
        try {
            driver.get("file:///C:/Users/dell/OneDrive/Desktop/test_select.html");
            WebElement selectElement = driver.findElement(By.id("mySelect"));
            Select select = new Select(selectElement);

            // Run test cases
            testOption(select, "Visible Option", true);
            testOption(select, "Hidden Option (display:none)", false);
            testOption(select, "Hidden Option (visibility:hidden)", false);
            testOption(select, "Hidden Option (opacity:0)", false);

            System.out.println("✅ Test execution completed. Press Enter to close the browser...");
            new Scanner(System.in).nextLine();
        } finally {
            driver.quit();
        }
    }

    private static void testOption(Select select, String visibleText, boolean shouldSucceed) {
        try {
            WebElement option = findOptionByText(select, visibleText);
            if (option != null && isVisible(option)) {
                select.selectByVisibleText(visibleText);
                if (shouldSucceed) {
                    System.out.println("✅ PASSED: Successfully selected: " + visibleText);
                } else {
                    System.out.println("❌ FAILED: Should NOT have been able to select: " + visibleText);
                }
            } else {
                if (shouldSucceed) {
                    System.out.println("❌ FAILED: Could NOT select: " + visibleText + " (option not visible)");
                } else {
                    System.out.println("✅ PASSED: Correctly failed to select: " + visibleText + " (option not visible)");
                }
            }
        } catch (NoSuchElementException e) {
            if (shouldSucceed) {
                System.out.println("❌ FAILED: Could NOT select: " + visibleText + " → " + e.getMessage());
            } else {
                System.out.println("✅ PASSED: Correctly failed to select: " + visibleText + " → " + e.getMessage());
            }
        } catch (Exception e) {
            System.out.println("⚠️ ERROR: Unexpected error while selecting: " + visibleText + " → " + e.getMessage());
        }
    }

    private static WebElement findOptionByText(Select select, String text) {
        List<WebElement> options = select.getOptions();
        for (WebElement option : options) {
            if (text.equals(option.getText())) {
                return option;
            }
        }
        return null;
    }

    private static boolean isVisible(WebElement element) {
        try {
            String visibility = element.getCssValue("visibility");
            String display = element.getCssValue("display");
            String opacity = element.getCssValue("opacity");
            return !visibility.equals("hidden") && !display.equals("none") && !opacity.equals("0") && !opacity.equals("0.0");
        } catch (Exception e) {
            return false;
        }
    }
}
