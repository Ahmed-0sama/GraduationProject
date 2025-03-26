import sys
import json
import re  # FIX: Import regex module
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager

def setup_driver():
    options = webdriver.ChromeOptions()
    options.add_argument('--headless')
    options.add_argument("--disable-gpu")
    options.add_argument("--window-size=1920,1080")
    options.add_argument("--disable-blink-features=AutomationControlled")
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    options.add_argument(
        "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36")
    service = Service(ChromeDriverManager().install())
    return webdriver.Chrome(service=service, options=options)

def clean_price(price_text):
    """Extracts only the numeric price and ensures it's valid."""
    numeric_price = re.sub(r"[^\d.]", "", price_text)  # Keep only numbers and dots
    if numeric_price.count(".") > 1:
        numeric_price = numeric_price.replace(".", "", numeric_price.count(".") - 1)  # Remove extra dots
    return numeric_price if numeric_price else "0"  # Ensure no empty string

def fetch_amazon_price(driver):
    try:
        driver.add_cookie({"name": "lc-main", "value": "en_US"})
        driver.add_cookie({"name": "lc-acbeg", "value": "en_AE"})
        driver.refresh()
        name_element = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, ".a-price-whole"))
        )
        price_text = name_element.text.strip()
        return clean_price(price_text)  # FIX: Clean the extracted price
    except Exception as e:
        return f"Amazon price not found: {e}"

def fetch_jumia_price(driver):
    try:
        cookies = [
            {"name": "_tt_enable_cookie", "value": "1", "domain": ".jumia.com.eg"},
            {"name": "userId", "value": "10973727", "domain": ".jumia.com.eg"},
            {"name": "customerUuid", "value": "562d4e6e-cd69-4a96-88b3-43ca0667e86b", "domain": ".jumia.com.eg"},
            {"name": "customerType", "value": "new", "domain": ".jumia.com.eg"},
            {"name": "accountType", "value": "Customer", "domain": ".jumia.com.eg"},
            {"name": "c_user", "value": "100007814165762", "domain": ".jumia.com.eg"},
            {"name": "userLanguage", "value": "en_EG", "domain": ".jumia.com.eg"}
        ]
        for cookie in cookies:
            driver.add_cookie(cookie)
        driver.refresh()
        name_element = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, ".-b.-ubpt.-tal.-fs24.-prxs"))
        )
        price_text = name_element.text.strip()
        return clean_price(price_text)
    except Exception as e:
        return f"Jumia price not found: {e}"

def fetch_noon_price(driver):
    try:
        name_element = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.XPATH, "//div[contains(@class, 'priceNow') or contains(@class, 'sellingPrice')]"))
        )
        price_text = name_element.text.strip()
        return clean_price(price_text)
    except Exception as e:
        return f"Noon price not found: {e}"

def main():
    if len(sys.argv) < 2:
        print("Error: No URL provided")
        sys.exit(1)

    url = sys.argv[1]
    driver = setup_driver()

    try:
        driver.get(url)
        url_lower = url.lower()
        price = ""

        if url_lower.startswith("https://www.amazon"):
            price = fetch_amazon_price(driver)
        elif url_lower.startswith("https://www.jumia"):
            price = fetch_jumia_price(driver)
        elif url_lower.startswith("https://www.noon"):
            price = fetch_noon_price(driver)
        else:
            print("Error: Unsupported website")
            sys.exit(1)

        json_output = json.dumps({"price": price}, indent=4)
        print(json_output)
    finally:
        driver.quit()

if __name__ == "__main__":
    main()
