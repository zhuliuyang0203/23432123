import pytest

from selenium.webdriver.common.bidi.network import Network


@pytest.mark.xfail_safari
def test_add_response_handler(driver, pages):
    passed = [False]

    def callback(response):
        if response.status_code == 200:
            passed[0] = True
        response.continue_response()

    driver.network.add_response_handler(callback)
    pages.load("basicAuth")
    assert passed[0], "Callback was NOT successful"

@pytest.mark.xfail_safari
def test_remove_response_handler(driver, pages):
    passed = [False]

    def callback(response):
        if response.status_code == 200:
            passed[0] = True
        response.continue_response()

    driver.network.add_response_handler(callback)
    driver.network.remove_response_handler(callback)
    pages.load("basicAuth")
    assert not passed[0], "Callback should NOT be successful"

@pytest.mark.xfail_safari
def test_add_request_handler(driver, pages):
    passed = [False]

    def callback(request):
        if request.method == 'GET':
            passed[0] = True
        request.continue_request()

    driver.network.add_request_handler(callback)
    pages.load("basicAuth")
    assert passed[0], "Callback was NOT successful"

@pytest.mark.xfail_safari
def test_remove_request_handler(driver, pages):
    passed = [False]

    def callback(request):
        if request.method == 'GET':
            passed[0] = True
        request.continue_request()

    driver.network.add_request_handler(callback)
    driver.network.remove_request_handler(callback)
    pages.load("basicAuth")
    assert not passed[0], "Callback should NOT be successful"

@pytest.mark.xfail_safari
def test_add_authentication_handler(driver, pages):
    driver.network.add_authentication_handler('test','test')
    pages.load("basicAuth")
    assert driver.find_element_by_tag_name('h1').text == 'authorized', "Authentication was NOT successful"

@pytest.mark.xfail_safari
def test_remove_authentication_handler(driver, pages):
    driver.network.add_authentication_handler('test', 'test')
    driver.network.remove_authentication_handler()
    pages.load("basicAuth")
    assert driver.find_element_by_tag_name('h1').text != 'authorized', "Authentication was successful"
