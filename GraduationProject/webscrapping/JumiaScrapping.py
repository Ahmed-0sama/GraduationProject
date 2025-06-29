import time
import random
import json
import sys
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager

# Set up Chrome options
options = Options()
options.add_argument("--headless")
options.add_argument("--disable-web-secruity")
options.add_argument("--blink-settings=imagesEnabled=false")
options.add_argument("--disable-blink-features=AutomationControlled")  # Avoid detection
options.add_argument(
    "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
)

if len(sys.argv) < 2:
    print("Error: No search query provided")
    sys.exit(1)

search_query = sys.argv[1] 

# Launch browser
service = Service(ChromeDriverManager().install())
driver = webdriver.Chrome(service=service, options=options)

# Open Jumia and set cookies
driver.get("https://www.jumia.com.eg/")
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

driver.get(f"https://www.jumia.com.eg/catalog/?q={search_query}")

WebDriverWait(driver, 1).until(
    EC.presence_of_element_located((By.CSS_SELECTOR, ".c-prd"))
)

# Simulate scrolling
for _ in range(1):
    driver.execute_script("window.scrollBy(0, 250);")
    time.sleep(random.uniform(0.5, 0.5))

products = driver.find_elements(By.CSS_SELECTOR, ".c-prd")

results = []

for product in products:
    try:
        driver.execute_script("arguments[0].scrollIntoView();", product)
        time.sleep(1)

        try:
            name_element = product.find_element(By.CSS_SELECTOR, ".name")
            name = name_element.text.strip()
        except:
            continue

        try:
            price_element = product.find_element(By.CSS_SELECTOR, ".prc")
            price = price_element.text.strip()
        except:
            price = "N/A"

        try:
            image_element = product.find_element(By.CSS_SELECTOR, ".img")
            image = image_element.get_attribute("data-src") or image_element.get_attribute("src")
        except:
            image = "N/A"

        try:
            link_element = product.find_element(By.CSS_SELECTOR, ".core")
            link = link_element.get_attribute("href")
        except:
            link = "N/A"

        # Append to results list
        results.append({
            "name": name,
            "price": price,
            "image": image,
            "link": link
        })

        # Stop after 10 items
        if len(results) >= 5:
            break
    except Exception:
        continue

# Print results as JSON
json_output = json.dumps(results, indent=4)
print(json_output)
sys.stdout.flush()

# Close browser
driver.quit()
