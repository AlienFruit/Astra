# ViewBoxLink Tag, JavaScript API, and Link Groups

This document provides comprehensive details on using `<view-box-link>` for dynamic navigation, the JavaScript API for programmatic control, and `<view-box-link-group>` for advanced link grouping and styling.

## 1. ViewBoxLink Tag

To create a dynamic link that loads content into a `<view-box>` without a full page refresh, use the `<view-box-link>` tag. This is ideal for navigation menus and any internal application links.

**Example usage:**

```html
<view-box-link id="home-link"
    selected-class-name="nav-link text-dark active"
    default-class-name="nav-link"
    view-box-id="main-view-box1"
    uri="/Home">
    Home
</view-box-link>
```

**ViewBoxLink Parameters:**

| Parameter | Type | Required | Description | Default Value |
|-----------|------|----------|-------------|---------------|
| `id` | `string` | ✅ Yes | Unique identifier for the view-box-link element | - |
| `uri` | `string` | ✅ Yes | URI address of the page to navigate to | - |
| `view-box-id` | `string` | ✅ Yes | ID of the view-box container where content will be loaded | - |
| `style` | `string` | ❌ No | Inline CSS styles for the element | `null` |
| `default-class-name` | `string` | ❌ No | CSS classes applied by default | `null` |
| `selected-class-name` | `string` | ❌ No | CSS classes applied when the link matches the current URL | `null` |
| `tag-name` | `string` | ❌ No | HTML tag name to use instead of `<a>` | `"a"` |
| `on-click-js-function` | `string` | ❌ No | Name of the JavaScript function to be called when the link is clicked | `null` |
| `scroll-up` | `bool` | ❌ No | Determines whether the page should scroll up after content loading | `true` |

## 2. JavaScript API for Programmatic Control

AlienFruit.Astra provides a JavaScript API that allows you to programmatically control view-boxes from your client-side code. This is useful for creating custom interactions, handling complex UI logic, or integrating with other JavaScript frameworks.

### ViewBoxRegistry.sendRequest() method

The `ViewBoxRegistry.sendRequest()` method allows you to dynamically load content into a specific view-box by sending an AJAX request:

```javascript
ViewBoxRegistry.sendRequest(viewBoxId, uri);
```

**Parameters:**
- `viewBoxId` (string): The ID of the target view-box container
- `uri` (string): The URI address of the page to navigate to

**Example usage in HTML:**
```html
<div id="dynamic-tab3-link"
    class="btn btn-primary"
    onclick="ViewBoxRegistry.sendRequest('dynamic-content-box', '/DynamicContent/Tab3');">
    Test request from JS
</div>
```

**Example usage in JavaScript:**
```javascript
// Load content into main view-box
ViewBoxRegistry.sendRequest('main-view-box1', '/Home/About');

// Load tab content dynamically
ViewBoxRegistry.sendRequest('dynamic-content-box', '/DynamicContent/Tab1');
```

### ViewBoxRegistry.abortCurrentRequest() method

Cancels the currently running request in a specific view-box:

```javascript
ViewBoxRegistry.abortCurrentRequest(viewBoxId);
```

**Parameters:**
- `viewBoxId` (string): The ID of the target view-box container

**Returns:**
- `boolean`: `true` if the request was successfully cancelled, `false` if there was no active request

**Benefits of programmatic control:**
- **Custom Interactions:** Create complex UI behaviors that go beyond simple links
- **Dynamic Content:** Load content based on user actions, form submissions, or application state
- **Integration:** Easily integrate with other JavaScript libraries and frameworks
- **Conditional Loading:** Load content based on business logic or user permissions
- **Request Cancellation:** Ability to cancel ongoing requests when needed

**Available JavaScript methods:**
- `ViewBoxRegistry.sendRequest(viewBoxId, uri)` - Load content into a view-box
- `ViewBoxRegistry.abortCurrentRequest(viewBoxId)` - Cancel current request in a view-box

**Additional ViewBoxRegistry methods:**
- `ViewBoxRegistry.create(id, specification)` - Create a new ViewBox instance
- `ViewBoxRegistry.getOrCreate(id, specification)` - Get or create a ViewBox instance
- `ViewBoxRegistry.get(id)` - Get an existing ViewBox instance
- `ViewBoxRegistry.isCreated(id)` - Check if ViewBox exists
- `ViewBoxRegistry.destroy(id)` - Destroy a ViewBox instance
- `ViewBoxRegistry.destroyAll()` - Destroy all ViewBox instances
- `ViewBoxRegistry.getOrOnReady(id, callback)` - Get ViewBox or call callback when ready

**Important notes:**
- If the view-box doesn't exist when calling `sendRequest()`, it will wait for the view-box to be created before executing the request
- If the view-box doesn't exist when calling `abortCurrentRequest()`, the method will return `false`
- All the same loading events and error handling apply as with view-box-link elements
- JavaScript API calls respect the same configuration settings (address changes, loading delays, etc.)

## 3. ViewBox Link Groups

The `<view-box-link-group>` tag allows you to create groups of navigation elements that share a common visual state. This is useful for creating navigation menus, tab groups, or any collection of links where you want to apply styling to a container element when any of the associated URIs is active.

**How view-box-link-group works:**
- Groups multiple URIs together under a single container element
- Automatically applies CSS classes to the group container based on the current page URI
- Monitors navigation events from the associated view-box
- Updates group styling when navigation occurs via view-box-link or JavaScript API

**Example usage:**

```html
<view-box-link-group id="main-nav-group"
    view-box-id="main-view-box1"
    default-class-name="nav-item"
    selected-class-name="nav-item active"
    uri-to-activete="@([
        "/Home", 
        "/About", 
        "/Contact"
    ])">
    <view-box-link id="home-link"
        default-class-name="nav-link"
        selected-class-name="nav-link active"
        view-box-id="main-view-box1"
        uri="/Home">
        Home
    </view-box-link>
    <view-box-link id="about-link"
        default-class-name="nav-link"
        selected-class-name="nav-link active"
        view-box-id="main-view-box1"
        uri="/About">
        About
    </view-box-link>
    <view-box-link id="contact-link"
        default-class-name="nav-link"
        selected-class-name="nav-link active"
        view-box-id="main-view-box1"
        uri="/Contact">
        Contact
    </view
</view-box-link-group>
```

**ViewBoxLinkGroup Parameters:**

| Parameter | Type | Required | Description | Default Value |
|-----------|------|----------|-------------|---------------|
| `id` | `string` | ✅ Yes | Unique identifier for the view-box-link-group element. Used for JavaScript interaction and specification lookup | - |
| `view-box-id` | `string` | ✅ Yes | ID of the view-box container associated with this group. The group will monitor navigation events for the specified view-box | - |
| `uri-to-activete` | `string[]` | ✅ Yes | Array of URIs that will activate the selected state for this group. When the current page URI matches any of these URIs, the SelectedClassName will be applied | - |
| `style` | `string` | ❌ No | Inline CSS styles for the group element | `null` |
| `default-class-name` | `string` | ❌ No | CSS classes applied by default to the group element when none of the monitored URIs are active | `null` |
| `selected-class-name` | `string` | ❌ No | CSS classes applied when at least one of the URIs from uri-to-activete is active | `null` |
| `tag-name` | `string` | ❌ No | HTML tag name to use for the group element | `"div"` |

**Use cases for ViewBoxLinkGroup:**
- **Navigation Menus:** Highlight menu sections when any page within that section is active
- **Tab Groups:** Style tab containers based on which tab is currently displayed
- **Breadcrumb Groups:** Apply styling to breadcrumb containers for related pages
- **Category Navigation:** Group related pages together and style the category container

**Benefits:**
- **Centralized Styling:** Apply styles to a container element instead of managing individual links
- **Automatic Updates:** Group styling updates automatically when navigation occurs
- **Flexible Grouping:** Group any combination of URIs together, regardless of their structure
- **Server-Side Rendering:** Initial CSS classes are applied on the server based on the current request path

**Important notes:**
- The `uri-to-activete` parameter must contain at least one URI
- URI matching is case-insensitive
- The group monitors navigation events from the specified view-box
- Both server-side (initial page load) and client-side (AJAX navigation) styling updates are supported
- The group element can contain any HTML content, including view-box-link elements
