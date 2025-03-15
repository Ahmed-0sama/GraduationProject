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

# Set cookie
driver.get("https://www.amazon.eg")
driver.add_cookie({"name": "lc-main", "value": "en_US"})
driver.add_cookie({"name": "lc-acbeg", "value": "en_AE"})
driver.get(f"https://www.amazon.eg/s?k={search_query}")

WebDriverWait(driver, 10).until(
    EC.presence_of_element_located((By.CSS_SELECTOR, "div.s-main-slot div.s-result-item"))
)

# Simulate scrolling
for _ in range(3):
    driver.execute_script("window.scrollBy(0, 1000);")
    time.sleep(random.uniform(2, 4))

products = driver.find_elements(By.CSS_SELECTOR, "div.s-main-slot div.s-result-item")

results = []

for product in products:
    try:
        driver.execute_script("arguments[0].scrollIntoView();", product)
        time.sleep(1)  # Wait for price to appear

        try:
            name_element = product.find_element(By.CSS_SELECTOR, "h2.a-size-base-plus.a-text-normal > span")
            name = name_element.text.strip()
        except:
            continue

        try:
            whole_part = product.find_element(By.CSS_SELECTOR, "span.a-price-whole").text.strip()
            decimal_part = product.find_element(By.CSS_SELECTOR, "span.a-price-fraction").text.strip()
            price = f"{whole_part}.{decimal_part} EGP"
        except:
            price = "N/A"

        try:
            image_element = product.find_element(By.CSS_SELECTOR, "img.s-image")
            image = image_element.get_attribute("src")
        except:
            image = "N/A"

        try:
            link_element = product.find_element(By.CSS_SELECTOR, "a.a-link-normal.s-no-outline")
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
    except Exception as e:
        continue

# Print results as JSON
json_output = json.dumps(results, indent=4)
print(json_output)
sys.stdout.flush()  # Ensure output is sent immediately
driver.quit()