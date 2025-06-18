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
options.add_argument("--headless")  # Run in headless mode
options.add_argument("--disable-blink-features=AutomationControlled")  # Avoid detection
options.add_argument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36")

if len(sys.argv) < 2:
    print("Error: No search query provided")
    sys.exit(1)

search_query = sys.argv[1]
search_url = f"https://www.noon.com/egypt-en/search/?q={search_query}"

# Launch browser
service = Service(ChromeDriverManager().install())
driver = webdriver.Chrome(service=service, options=options)
driver.get(search_url)

try:
    # Wait for product grid to load
    WebDriverWait(driver, 10).until(
        EC.presence_of_element_located((By.CSS_SELECTOR, ".ProductBoxLinkHandler_linkWrapper__b0qZ9"))
    )

    # Simulate scrolling
    for _ in range(3):
        driver.execute_script("window.scrollBy(0, 1000);")
        time.sleep(random.uniform(2, 4))

    products = driver.find_elements(By.CSS_SELECTOR, ".ProductBoxLinkHandler_linkWrapper__b0qZ9")
    results = []

    for product in products:
        try:
            driver.execute_script("arguments[0].scrollIntoView();", product)
            time.sleep(1)

            try:
                name = product.find_element(By.CSS_SELECTOR, ".ProductDetailsSection_wrapper__yLBrw h2").text.strip()
            except:
                continue

            try:
                price = product.find_element(By.CSS_SELECTOR, ".Price_sellingPrice__HFKZf").text.strip()
            except:
                price = "N/A"

            try:
                img_element = product.find_element(By.CSS_SELECTOR, ".ProductImage_imageWrapper__C_aHA img")
                image = img_element.get_attribute("src") or img_element.get_attribute("data-src")
            except:
                image = "N/A"

            try:
                link_element = product.find_element(By.CSS_SELECTOR, ".ProductBoxLinkHandler_productBoxLink__FPhjp")
                link = link_element.get_attribute("href")
            except:
                link = "No Link Available"

            results.append({
                "name": name,
                "price": price,
                "image": image,
                "link": link
            })

            if len(results) >= 5:
                break

        except Exception as e:
            continue

    print(json.dumps(results, indent=4))
    sys.stdout.flush()

except Exception as e:
    print(json.dumps({"error": str(e)}))

finally:
    driver.quit()
