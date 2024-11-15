import pytest

from selenium import webdriver
from selenium.webdriver.common.bidi.network import Network
from selenium.webdriver.common.bidi.network import Response
from selenium.webdriver.common.bidi.network import Request


@pytest.fixture
def network(driver):
    yield Network(driver)

def test_add_response_handler(network):
    passed = [False]

    def callback(response):
        if response.status_code == 200:
            passed[0] = True
        response.continue_response()

    network.add_response_handler(callback)
    pages.load("basicAuth")
    assert passed[0] == True, "Callback was NOT successful"

def test_remove_response_handler(network):
    passed = [False]

    def callback(response):
        if response.status_code == 200:
            passed[0] = True
        response.continue_response()

    network.add_response_handler(callback)
    network.remove_response_handler(callback)
    pages.load("basicAuth")
    assert passed[0] == False, "Callback was successful"

def test_add_request_handler(request):
    passed = [False]

    def callback(request):
        if request.method == 'GET':
            passed[0] = True
        request.continue_request()

    network_request.add_request_handler(callback)
    pages.load("basicAuth")
    assert passed[0] == True, "Callback was NOT successful"

def test_remove_request_handler(request):
    passed = [False]

    def callback(request):
        if request.method == 'GET':
            passed[0] = True
        request.continue_request()

    network_request.add_request_handler(callback)
    network_request.remove_request_handler(callback)
    pages.load("basicAuth")
    assert passed[0] == False, "Callback was successful"

def test_add_authentication_handler(network):
    network.add_authentication_handler('test','test')
    pages.load("basicAuth")
    assert driver.find_element_by_tag_name('h1').text == 'authorized', "Authentication was NOT successful"

def test_remove_authentication_handler(network):
    network.add_authentication_handler('test', 'test')
    network.remove_authentication_handler()
    pages.load("basicAuth")
    assert driver.find_element_by_tag_name('h1').text != 'authorized', "Authentication was successful"