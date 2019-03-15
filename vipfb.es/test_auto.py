from selenium import webdriver
from selenium.webdriver.firefox.firefox_binary import FirefoxBinary
import selenium.webdriver.chrome.service as service
from selenium.webdriver.common.proxy import Proxy
from selenium.webdriver.common.proxy import ProxyType
import selenium.webdriver.chrome.service as service
from selenium.webdriver.common.action_chains import ActionChains
import time
import imageio
from requests.auth import HTTPBasicAuth
from PIL import Image

import os
import urllib
import requests
import pickle
from io import StringIO
import base64

def get_captcha(driver, element, path):
    # now that we have the preliminary stuff out of the way time to get that image :D
    location = element.location
    size = element.size
    # saves screenshot of entire page
    driver.save_screenshot(path)

    # uses PIL library to open image in memory
    image = Image.open(path)

    left = location['x'] + 167
    top = location['y'] - 957
    right = left + size['width']
    bottom = top + size['height']

    print(left, top, right, bottom)

    image = image.crop((left, top, right, bottom))  # defines crop points
    #image = image.crop((845, 117, 845 + size['width'], 117 + size['height']))
    image = image.convert("RGB")
    image.save(path, 'png')  # saves new cropped image

    image.show()

    # box = (int(left), int(top), int(right), int(bottom))
    # screenshot = driver.get_screenshot_as_base64()
    # img = Image.open(StringIO(base64.b64decode(screenshot)))
    # area = img.crop(box)
    # area.save('screenshot.png', 'PNG')

proxy = Proxy(
     {
          'proxyType': ProxyType.MANUAL,
          'httpProxy': 'ip_or_host:port'
     }
)

# service = service.Service('chromedriver.exe')
# service.start()
# capabilities = {'chrome.binary': '/path/to/custom/chrome'}
# driver = webdriver.Remote(service.service_url, capabilities)

driver = webdriver.Firefox()
driver.get('https://vipfb.es/Request')
driver.maximize_window()
cookies = driver.get_cookies()
#driver.switch_to.frame("Main")


#while True:
time.sleep(2)
#print(cookies)
img = driver.find_elements_by_tag_name("img")
driver.execute_script("arguments[0].scrollIntoView();", img[0])
#actions = ActionChains(driver)
#actions.move_to_element(img).perform()
get_captcha(driver, img[0], "captcha.png")
src = img[0].get_attribute("src")
print(src)
print(len(img))

# r = requests.get(src, cookies=cookies[2])

# with open("python1.png", "wb") as code:
#     code.write(r.content)
time.sleep(5)

driver.close()